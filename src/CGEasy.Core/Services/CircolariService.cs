using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Service per gestione circolari con file system (ASYNC)
    /// Gestisce salvataggio/spostamento/eliminazione file
    /// </summary>
    public class CircolariService
    {
        private readonly CircolariRepository _circolariRepo;
        private readonly ArgomentiRepository _argomentiRepo;
        private readonly string _basePath;

        public CircolariService(CGEasyDbContext context)
        {
            _circolariRepo = new CircolariRepository(context);
            _argomentiRepo = new ArgomentiRepository(context);

            // Percorso base: C:\db_CGEASY\Circolari
            _basePath = Path.Combine(@"C:\db_CGEASY", "Circolari");

            // Assicura esistenza cartella base
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <summary>
        /// Importa una nuova circolare con file (ASYNC)
        /// </summary>
        public async Task<int> ImportaCircolareAsync(int argomentoId, string descrizione, int anno, string sourceFilePath, int utenteId)
        {
            // Validazione
            if (argomentoId <= 0) throw new ArgumentException("Argomento non valido");
            if (string.IsNullOrWhiteSpace(descrizione)) throw new ArgumentException("Descrizione obbligatoria");
            if (anno < 1900 || anno > 2100) throw new ArgumentException("Anno non valido");
            if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("File non trovato", sourceFilePath);

            // Ottiene nome argomento
            var argomento = await _argomentiRepo.GetByIdAsync(argomentoId);
            if (argomento == null) throw new ArgumentException("Argomento non trovato");

            // Crea struttura cartelle: Circolari\[Anno]\[Argomento]
            var targetDirectory = Path.Combine(_basePath, anno.ToString(), argomento.Nome);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Copia file (con gestione duplicati)
            var originalFileName = Path.GetFileName(sourceFilePath);
            var targetFilePath = Path.Combine(targetDirectory, originalFileName);
            
            // Se file esiste già, aggiungi timestamp
            if (File.Exists(targetFilePath))
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                var extension = Path.GetExtension(originalFileName);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                targetFilePath = Path.Combine(targetDirectory, $"{fileNameWithoutExt}_{timestamp}{extension}");
            }

            File.Copy(sourceFilePath, targetFilePath, overwrite: false);

            // Percorso relativo per DB (es: 2024\Fiscale\file.pdf)
            var percorsoRelativo = Path.Combine(anno.ToString(), argomento.Nome, Path.GetFileName(targetFilePath));

            // Crea record database
            var circolare = new Circolare
            {
                ArgomentoId = argomentoId,
                Descrizione = descrizione,
                Anno = anno,
                NomeFile = Path.GetFileName(targetFilePath),
                PercorsoFile = percorsoRelativo,
                DataImportazione = DateTime.Now,
                UtenteId = utenteId
            };

            return await _circolariRepo.InsertAsync(circolare);
        }

        /// <summary>
        /// Modifica circolare esistente (ASYNC)
        /// </summary>
        public async Task<bool> ModificaCircolareAsync(int circolareId, int nuovoArgomentoId, string nuovaDescrizione, int nuovoAnno)
        {
            // Validazione
            if (nuovoArgomentoId <= 0) throw new ArgumentException("Argomento non valido");
            if (string.IsNullOrWhiteSpace(nuovaDescrizione)) throw new ArgumentException("Descrizione obbligatoria");
            if (nuovoAnno < 1900 || nuovoAnno > 2100) throw new ArgumentException("Anno non valido");

            var circolare = await _circolariRepo.GetByIdAsync(circolareId);
            if (circolare == null) return false;

            var argomento = await _argomentiRepo.GetByIdAsync(nuovoArgomentoId);
            if (argomento == null) throw new ArgumentException("Argomento non trovato");

            // Verifica se serve spostare il file
            bool serveSpotareFile = (circolare.ArgomentoId != nuovoArgomentoId || circolare.Anno != nuovoAnno);

            if (serveSpotareFile)
            {
                // Percorso vecchio
                var vecchioPercorsoCompleto = Path.Combine(_basePath, circolare.PercorsoFile);

                // Nuovo percorso
                var nuovaDirectory = Path.Combine(_basePath, nuovoAnno.ToString(), argomento.Nome);
                if (!Directory.Exists(nuovaDirectory))
                {
                    Directory.CreateDirectory(nuovaDirectory);
                }

                var nuovoPercorsoCompleto = Path.Combine(nuovaDirectory, circolare.NomeFile);

                // Sposta file se esiste
                if (File.Exists(vecchioPercorsoCompleto))
                {
                    // Se destinazione esiste già, aggiungi timestamp
                    if (File.Exists(nuovoPercorsoCompleto))
                    {
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(circolare.NomeFile);
                        var extension = Path.GetExtension(circolare.NomeFile);
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var nuovoNomeFile = $"{fileNameWithoutExt}_{timestamp}{extension}";
                        nuovoPercorsoCompleto = Path.Combine(nuovaDirectory, nuovoNomeFile);
                        circolare.NomeFile = nuovoNomeFile;
                    }

                    File.Move(vecchioPercorsoCompleto, nuovoPercorsoCompleto);

                    // Aggiorna percorso relativo
                    circolare.PercorsoFile = Path.Combine(nuovoAnno.ToString(), argomento.Nome, circolare.NomeFile);
                }
            }

            // Aggiorna metadati
            circolare.ArgomentoId = nuovoArgomentoId;
            circolare.Descrizione = nuovaDescrizione;
            circolare.Anno = nuovoAnno;

            return await _circolariRepo.UpdateAsync(circolare);
        }

        /// <summary>
        /// Elimina circolare (ASYNC)
        /// </summary>
        public async Task<bool> EliminaCircolareAsync(int circolareId)
        {
            var circolare = await _circolariRepo.GetByIdAsync(circolareId);
            if (circolare == null) return false;

            // Elimina file fisico
            var percorsoCompleto = Path.Combine(_basePath, circolare.PercorsoFile);
            if (File.Exists(percorsoCompleto))
            {
                try
                {
                    File.Delete(percorsoCompleto);
                }
                catch (Exception ex)
                {
                    // Log errore ma continua con eliminazione DB
                    System.Diagnostics.Debug.WriteLine($"Errore eliminazione file: {ex.Message}");
                }
            }

            // Elimina record database
            return await _circolariRepo.DeleteAsync(circolareId);
        }

        /// <summary>
        /// Apre il file della circolare con applicazione predefinita (ASYNC)
        /// </summary>
        public async Task<bool> ApriCircolareAsync(int circolareId)
        {
            var circolare = await _circolariRepo.GetByIdAsync(circolareId);
            if (circolare == null) return false;

            var percorsoCompleto = Path.Combine(_basePath, circolare.PercorsoFile);
            if (!File.Exists(percorsoCompleto)) return false;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = percorsoCompleto,
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ottiene il percorso completo del file (ASYNC)
        /// </summary>
        public async Task<string?> GetPercorsoCompletoAsync(int circolareId)
        {
            var circolare = await _circolariRepo.GetByIdAsync(circolareId);
            if (circolare == null) return null;

            return Path.Combine(_basePath, circolare.PercorsoFile);
        }

        /// <summary>
        /// Verifica se il file esiste fisicamente (ASYNC)
        /// </summary>
        public async Task<bool> FileExistsAsync(int circolareId)
        {
            var percorso = await GetPercorsoCompletoAsync(circolareId);
            return percorso != null && File.Exists(percorso);
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public int ImportaCircolare(int argomentoId, string descrizione, int anno, string sourceFilePath, int utenteId)
        {
            return ImportaCircolareAsync(argomentoId, descrizione, anno, sourceFilePath, utenteId).GetAwaiter().GetResult();
        }

        public bool ModificaCircolare(int circolareId, int nuovoArgomentoId, string nuovaDescrizione, int nuovoAnno)
        {
            return ModificaCircolareAsync(circolareId, nuovoArgomentoId, nuovaDescrizione, nuovoAnno).GetAwaiter().GetResult();
        }

        public bool EliminaCircolare(int circolareId)
        {
            return EliminaCircolareAsync(circolareId).GetAwaiter().GetResult();
        }

        public bool ApriCircolare(int circolareId)
        {
            return ApriCircolareAsync(circolareId).GetAwaiter().GetResult();
        }
    }
}
