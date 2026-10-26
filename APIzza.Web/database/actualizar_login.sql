-- Ejecutar una sola vez en DBeaver si ya tenés creada la base APIzza.
USE apizza;
ALTER TABLE Clientes ADD COLUMN PasswordHash VARCHAR(500) NULL;
