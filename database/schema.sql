-- APIzza - estructura MySQL
-- Ejecutar este archivo PRIMERO.
-- Compatible con MySQL 8.x y pensado para ejecutarse completo desde DBeaver.

CREATE DATABASE IF NOT EXISTS apizza
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE apizza;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS ItemsPedido;
DROP TABLE IF EXISTS Pedidos;
DROP TABLE IF EXISTS Clientes;
DROP TABLE IF EXISTS Pizzas;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE Pizzas (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(120) NOT NULL,
    Descripcion VARCHAR(500) NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Categoria VARCHAR(30) NOT NULL DEFAULT 'clasica',
    PRIMARY KEY (Id),
    INDEX IX_Pizzas_Categoria (Categoria)
) ENGINE=InnoDB;

CREATE TABLE Clientes (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(120) NOT NULL,
    Email VARCHAR(160) NOT NULL,
    Telefono VARCHAR(40) NULL,
    Direccion VARCHAR(250) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Clientes_Email (Email)
) ENGINE=InnoDB;

CREATE TABLE Pedidos (
    Id INT NOT NULL AUTO_INCREMENT,
    ClienteId INT NOT NULL,
    Estado VARCHAR(30) NOT NULL DEFAULT 'pendiente',
    Total DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CreadoEn DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    INDEX IX_Pedidos_ClienteId (ClienteId)
) ENGINE=InnoDB;

CREATE TABLE ItemsPedido (
    Id INT NOT NULL AUTO_INCREMENT,
    PedidoId INT NOT NULL,
    PizzaId INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_ItemsPedido_PedidoId (PedidoId),
    INDEX IX_ItemsPedido_PizzaId (PizzaId)
) ENGINE=InnoDB;

-- Claves foráneas agregadas por separado para evitar problemas de parser en DBeaver.
ALTER TABLE Pedidos
    ADD CONSTRAINT FK_Pedidos_Clientes
    FOREIGN KEY (ClienteId) REFERENCES Clientes (Id)
    ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE ItemsPedido
    ADD CONSTRAINT FK_ItemsPedido_Pedidos
    FOREIGN KEY (PedidoId) REFERENCES Pedidos (Id)
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE ItemsPedido
    ADD CONSTRAINT FK_ItemsPedido_Pizzas
    FOREIGN KEY (PizzaId) REFERENCES Pizzas (Id)
    ON DELETE RESTRICT ON UPDATE CASCADE;

-- Validación básica del lado de la BD.
ALTER TABLE ItemsPedido
    ADD CONSTRAINT CK_ItemsPedido_Cantidad CHECK (Cantidad > 0);
