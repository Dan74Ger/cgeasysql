using System;
using System.IO;
using System.Linq;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Service per gestione circolari con file system
    /// Gestisce salvataggio/spostamento/eliminazione file
    /// </summary>
    public class CircolariService
    {
        private readonly CircolariRepository _circolariRepo;
        private readonly ArgomentiRepository _argomentiRepo;
        private readonly string _basePath;

        public CircolariService(LiteDbContext context)
        {
            _circolariRepo = new CircolariRepository(context);
            _argomentiRepo = new ArgomentiRepository(context);

            // Percorso base: stessa cartella del database + \Circolari
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var dbDirectory = Path.GetDirectoryName(dbPath) ?? @"C:\devcg-group\dbtest_prova";
            _basePath = Path.Combine(dbDirectory, "Circolari");

            // Assicura esistenza cartella base
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <summary>
        /// Importa una nuova circolare con file
        /// </summary>
        public int ImportaCircolare(int argomentoId, string descrizione, int anno, string sourceFilePath, int utenteId)
        {
            // Validazione
            if (argomentoId <= 0) throw new ArgumentException("Argomento non valido");
            if (string.IsNullOrWhiteSpace(descrizione)) throw new ArgumentException("Descrizione obbligatoria");
            if (anno < 1900 || anno > 2100) throw new ArgumentException("Anno non valido");
            if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("File non trovato", sourceFilePath);

            // Ottiene nome argomento
            var argomento = _argomentiRepo.GetById(argomentoId);
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

            return _circolariRepo.Insert(circolare);
        }

        /// <summary>
        /// Modifica circolare esistente (aggiorna metadati e sposta file se necessario)
        /// </summary>
        public bool ModificaCircolare(int circolareId, int nuovoArgomentoId, string nuovaDescrizione, int nuovoAnno)
        {
            // Validazione
            if (nuovoArgomentoId <= 0) throw new ArgumentException("Argomento non valido");
            if (string.IsNullOrWhiteSpace(nuovaDescrizione)) throw new ArgumentException("Descrizione obbligatoria");
            if (nuovoAnno < 1900 || nuovoAnno > 2100) throw new ArgumentException("Anno non valido");

            var circolare = _circolariRepo.GetById(circolareId);
            if (circolare == null) return false;

            var argomento = _argomentiRepo.GetById(nuovoArgomentoId);
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

            return _circolariRepo.Update(circolare);
        }

        /// <summary>
        /// Elimina circolare (record DB + file fisico)
        /// </summary>
        public bool EliminaCircolare(int circolareId)
        {
            var circolare = _circolariRepo.GetById(circolareId);
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
            return _circolariRepo.Delete(circolareId);
        }

        /// <summary>
        /// Apre il file della circolare con applicazione predefinita
        /// </summary>
        public bool ApriCircolare(int circolareId)
        {
            var circolare = _circolariRepo.GetById(circolareId);
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
        /// Ottiene il percorso completo del file
        /// </summary>
        public string? GetPercorsoCompleto(int circolareId)
        {
            var circolare = _circolariRepo.GetById(circolareId);
            if (circolare == null) return null;

            return Path.Combine(_basePath, circolare.PercorsoFile);
        }

        /// <summary>
        /// Verifica se il file esiste fisicamente
        /// </summary>
        public bool FileExists(int circolareId)
        {
            var percorso = GetPercorsoCompleto(circolareId);
            return percorso != null && File.Exists(percorso);
        }
    }
}

