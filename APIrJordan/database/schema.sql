-- APIr Jordan - estructura MySQL
-- Ejecutar este archivo PRIMERO (después inserts.sql y, para probar, consultas.sql).
-- Compatible con MySQL 8.x y pensado para ejecutarse completo desde DBeaver o MySQL Workbench.
--
-- Importante: los nombres de tablas y columnas son EXACTAMENTE los que usa Entity Framework
-- en APIrJordan.Web/Data/ApirJordanDbContext.cs. Si la app arranca contra una base vacía,
-- crea estas mismas tablas sola (EnsureCreated); este script existe para poder revisar y
-- defender la estructura sin depender del código.

CREATE DATABASE IF NOT EXISTS apirjordan
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE apirjordan;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS ItemsPedido;
DROP TABLE IF EXISTS Pedidos;
DROP TABLE IF EXISTS Clientes;
DROP TABLE IF EXISTS StockTalles;
DROP TABLE IF EXISTS Zapatillas;

SET FOREIGN_KEY_CHECKS = 1;

-- Catálogo: un registro por modelo/colorway. El stock NO vive acá, vive en StockTalles.
CREATE TABLE Zapatillas (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(120) NOT NULL,
    Colorway VARCHAR(80) NULL,
    Descripcion VARCHAR(600) NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Categoria VARCHAR(30) NOT NULL DEFAULT 'retro',     -- retro | lifestyle | basquet
    Imagen VARCHAR(120) NULL,                             -- nombre de archivo en wwwroot/images/zapatillas
    Destacada TINYINT(1) NOT NULL DEFAULT 0,              -- aparece en la portada
    Activa TINYINT(1) NOT NULL DEFAULT 1,                 -- 0 = pausada: no se muestra ni se vende
    CreadoEn DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    INDEX IX_Zapatillas_Categoria (Categoria)
) ENGINE=InnoDB;

-- Stock por talle: una fila por (modelo, talle). Es la tabla que hace distinto al proyecto.
CREATE TABLE StockTalles (
    Id INT NOT NULL AUTO_INCREMENT,
    ZapatillaId INT NOT NULL,
    Talle DECIMAL(3,1) NOT NULL,                          -- 38.0 ... 45.0, admite medios (40.5, 42.5)
    Cantidad INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_StockTalles_ZapatillaId_Talle (ZapatillaId, Talle),
    INDEX IX_StockTalles_ZapatillaId (ZapatillaId)
) ENGINE=InnoDB;

CREATE TABLE Clientes (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(120) NOT NULL,
    Email VARCHAR(160) NOT NULL,
    Telefono VARCHAR(40) NULL,
    Direccion VARCHAR(250) NOT NULL,
    CreadoEn DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Clientes_Email (Email)
) ENGINE=InnoDB;

CREATE TABLE Pedidos (
    Id INT NOT NULL AUTO_INCREMENT,
    ClienteId INT NOT NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'pendiente',      -- pendiente > pagado > enviado > entregado | cancelado
    Total DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    Nota VARCHAR(300) NULL,
    CreadoEn DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ActualizadoEn DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    INDEX IX_Pedidos_ClienteId (ClienteId),
    INDEX IX_Pedidos_Estado (Estado)
) ENGINE=InnoDB;

-- Líneas del pedido. Guardan el talle y el precio del momento de la compra (histórico).
CREATE TABLE ItemsPedido (
    Id INT NOT NULL AUTO_INCREMENT,
    PedidoId INT NOT NULL,
    ZapatillaId INT NOT NULL,
    Talle DECIMAL(3,1) NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_ItemsPedido_PedidoId (PedidoId),
    INDEX IX_ItemsPedido_ZapatillaId (ZapatillaId)
) ENGINE=InnoDB;

-- Claves foráneas agregadas por separado para evitar problemas de parser en DBeaver.
ALTER TABLE StockTalles
    ADD CONSTRAINT FK_StockTalles_Zapatillas
    FOREIGN KEY (ZapatillaId) REFERENCES Zapatillas (Id)
    ON DELETE CASCADE ON UPDATE CASCADE;          -- se borra el modelo => se borra su stock

ALTER TABLE Pedidos
    ADD CONSTRAINT FK_Pedidos_Clientes
    FOREIGN KEY (ClienteId) REFERENCES Clientes (Id)
    ON DELETE RESTRICT ON UPDATE CASCADE;         -- no se puede borrar un cliente con pedidos

ALTER TABLE ItemsPedido
    ADD CONSTRAINT FK_ItemsPedido_Pedidos
    FOREIGN KEY (PedidoId) REFERENCES Pedidos (Id)
    ON DELETE CASCADE ON UPDATE CASCADE;          -- las líneas no tienen sentido sin su pedido

ALTER TABLE ItemsPedido
    ADD CONSTRAINT FK_ItemsPedido_Zapatillas
    FOREIGN KEY (ZapatillaId) REFERENCES Zapatillas (Id)
    ON DELETE RESTRICT ON UPDATE CASCADE;         -- no se puede borrar un modelo que ya se vendió

-- Validaciones del lado de la base (la app también las controla, pero la BD es la última barrera).
ALTER TABLE Zapatillas
    ADD CONSTRAINT CK_Zapatillas_Precio CHECK (Precio > 0);

ALTER TABLE StockTalles
    ADD CONSTRAINT CK_StockTalles_Cantidad CHECK (Cantidad >= 0);   -- nunca stock negativo

ALTER TABLE StockTalles
    ADD CONSTRAINT CK_StockTalles_Talle CHECK (Talle BETWEEN 35 AND 50);

ALTER TABLE ItemsPedido
    ADD CONSTRAINT CK_ItemsPedido_Cantidad CHECK (Cantidad > 0);

ALTER TABLE ItemsPedido
    ADD CONSTRAINT CK_ItemsPedido_PrecioUnitario CHECK (PrecioUnitario > 0);

ALTER TABLE Pedidos
    ADD CONSTRAINT CK_Pedidos_Estado CHECK (Estado IN ('pendiente', 'pagado', 'enviado', 'entregado', 'cancelado'));
