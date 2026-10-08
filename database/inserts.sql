-- APIzza - datos iniciales
-- Ejecutar este archivo DESPUÉS de schema.sql.

USE apizza;

INSERT INTO Pizzas (Nombre, Descripcion, Precio, Categoria) VALUES
('Muzzarella', 'La clásica de siempre: salsa de tomate y muzzarella extra', 6500.00, 'clasica'),
('Napolitana', 'Muzzarella, tomate en rodajas, ajo y aceite de oliva', 7200.00, 'clasica'),
('Fugazzeta', 'Doble muzzarella y cebolla a la parrilla', 7500.00, 'especial'),
('Cuatro Quesos', 'Muzzarella, provolone, roquefort y parmesano', 8300.00, 'especial'),
('Vegetariana', 'Muzzarella, morrón, cebolla, aceitunas y choclo', 7800.00, 'vegetariana');

INSERT INTO Clientes (Nombre, Email, Telefono, Direccion) VALUES
('Cliente de prueba', 'cliente@apizza.local', '1122334455', 'Av. Siempre Viva 123');
SET @cliente_prueba_id = LAST_INSERT_ID();

INSERT INTO Pedidos (ClienteId, Estado, Total)
VALUES (@cliente_prueba_id, 'pendiente', 13700.00);
SET @pedido_prueba_id = LAST_INSERT_ID();

-- Pedido de prueba: 1 Muzzarella + 1 Napolitana
INSERT INTO ItemsPedido (PedidoId, PizzaId, Cantidad, PrecioUnitario)
VALUES
(@pedido_prueba_id, 1, 1, 6500.00),
(@pedido_prueba_id, 2, 1, 7200.00);
