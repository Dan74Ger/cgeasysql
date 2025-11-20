using System;
using BCrypt.Net;

Console.WriteLine("=== GENERAZIONE HASH BCRYPT PER UTENTI DEFAULT ===\n");

// Password admin: 123456
string hash1 = BCrypt.HashPassword("123456", 11);
Console.WriteLine("Hash per admin (password: 123456):");
Console.WriteLine(hash1);
Console.WriteLine();

// Password admin1: 123123
string hash2 = BCrypt.HashPassword("123123", 11);
Console.WriteLine("Hash per admin1 (password: 123123):");
Console.WriteLine(hash2);
Console.WriteLine();

Console.WriteLine("=== HASH GENERATI CON SUCCESSO ===");

