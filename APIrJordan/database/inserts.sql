-- APIr Jordan - datos de ejemplo
-- Ejecutar DESPUÉS de schema.sql. Es el mismo contenido que APIrJordan.Web/Data/SeedData.cs:
-- si la app arranca con la base vacía carga esto sola; si ya corriste este script, no duplica nada.
--
-- Los Ids se indican a mano para que las referencias entre tablas queden claras al leer el archivo.
-- Las fechas se calculan relativas a NOW() para que el panel y la portada muestren "hace N días".

USE apirjordan;

SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE ItemsPedido;
TRUNCATE TABLE Pedidos;
TRUNCATE TABLE Clientes;
TRUNCATE TABLE StockTalles;
TRUNCATE TABLE Zapatillas;
SET FOREIGN_KEY_CHECKS = 1;

-- ---------------------------------------------------------------
-- 1) Catálogo: 10 modelos (retro, lifestyle y básquet)
-- ---------------------------------------------------------------
INSERT INTO Zapatillas (Id, Nombre, Colorway, Descripcion, Precio, Categoria, Imagen, Destacada, Activa, CreadoEn) VALUES
  (1, 'Air Jordan 1 Retro High OG', 'Chicago', 'La silueta que empezó todo, en la combinación más famosa: cuero blanco, rojo y negro sobre la horma original de 1985.', 389999.00, 'retro', 'aj1-high-chicago.jpg', 1, 1, NOW() - INTERVAL 40 DAY),
  (2, 'Air Jordan 4 Retro', 'Bred Reimagined', 'Nubuck negro, mallas laterales y los detalles en rojo del par original de 1989. Amortiguación Air visible en el talón.', 419999.00, 'retro', 'aj4-bred.jpg', 1, 1, NOW() - INTERVAL 35 DAY),
  (3, 'Air Jordan 11 Retro', 'Concord', 'Charol negro sobre malla blanca y suela translúcida. La más elegante de la línea, pensada para la cancha.', 449999.00, 'retro', 'aj11-concord.jpg', 1, 1, NOW() - INTERVAL 30 DAY),
  (4, 'Air Jordan 3 Retro', 'White Cement', 'Cuero blanco con el clásico estampado elefante en puntera y talón. La primera con Air visible.', 399999.00, 'retro', 'aj3-white-cement.jpg', 0, 1, NOW() - INTERVAL 28 DAY),
  (5, 'Air Jordan 1 Low', 'Bred Toe', 'La versión baja de la 1, liviana y fácil de usar todos los días. Puntera roja, paneles negros y cuarto blanco.', 249999.00, 'lifestyle', 'aj1-low-bred-toe.jpg', 1, 1, NOW() - INTERVAL 21 DAY),
  (6, 'Jordan Spizike Low', 'White/Black', 'Un híbrido que mezcla detalles de la 3, la 4, la 5 y la 6 en una silueta baja. Cuero blanco con acentos negros.', 299999.00, 'lifestyle', 'spizike-low.jpg', 0, 1, NOW() - INTERVAL 18 DAY),
  (7, 'Jordan Max Aura 6', 'Black/Red', 'Inspirada en la Air Jordan 3 pero con unidad Max Air en el talón. Cómoda para caminar todo el día.', 219999.00, 'lifestyle', 'max-aura-6.jpg', 0, 1, NOW() - INTERVAL 14 DAY),
  (8, 'Jordan Luka 3', 'Motorsport', 'Tejido reforzado, espuma Formula 23 y una base ancha para frenar y cambiar de dirección.', 259999.00, 'basquet', 'luka-3.jpg', 0, 1, NOW() - INTERVAL 10 DAY),
  (9, 'Jordan Tatum 3', 'Zen', 'Liviana y de perfil bajo. Malla técnica verde salvia y suela de goma natural.', 239999.00, 'basquet', 'tatum-3.jpg', 0, 1, NOW() - INTERVAL 7 DAY),
  (10, 'Air Jordan 12 Retro', 'Taxi', 'Cuero blanco con costuras radiales y guardabarros negro. Edición agotada: avisamos cuando vuelva.', 429999.00, 'retro', 'aj12-taxi.jpg', 0, 1, NOW() - INTERVAL 3 DAY);

-- ---------------------------------------------------------------
-- 2) Stock por talle: 10 filas por modelo (curva 38 a 45), aunque la cantidad sea 0.
--    Las cantidades ya tienen descontadas las ventas de los pedidos de ejemplo de más abajo.
--    La Air Jordan 12 "Taxi" (Id 10) queda con todo en 0: es el ejemplo de modelo agotado.
-- ---------------------------------------------------------------
INSERT INTO StockTalles (Id, ZapatillaId, Talle, Cantidad) VALUES
  -- Air Jordan 1 Retro High OG "Chicago"
  (1, 1, 38, 0), (2, 1, 39, 2), (3, 1, 40, 3), (4, 1, 40.5, 1), (5, 1, 41, 4), (6, 1, 42, 3), (7, 1, 42.5, 2), (8, 1, 43, 2), (9, 1, 44, 1), (10, 1, 45, 0),
  -- Air Jordan 4 Retro "Bred Reimagined"
  (11, 2, 38, 0), (12, 2, 39, 0), (13, 2, 40, 2), (14, 2, 40.5, 0), (15, 2, 41, 3), (16, 2, 42, 2), (17, 2, 42.5, 0), (18, 2, 43, 1), (19, 2, 44, 2), (20, 2, 45, 0),
  -- Air Jordan 11 Retro "Concord"
  (21, 3, 38, 0), (22, 3, 39, 0), (23, 3, 40, 0), (24, 3, 40.5, 0), (25, 3, 41, 1), (26, 3, 42, 2), (27, 3, 42.5, 0), (28, 3, 43, 2), (29, 3, 44, 0), (30, 3, 45, 1),
  -- Air Jordan 3 Retro "White Cement"
  (31, 4, 38, 0), (32, 4, 39, 0), (33, 4, 40, 1), (34, 4, 40.5, 0), (35, 4, 41, 2), (36, 4, 42, 1), (37, 4, 42.5, 0), (38, 4, 43, 0), (39, 4, 44, 1), (40, 4, 45, 0),
  -- Air Jordan 1 Low "Bred Toe"
  (41, 5, 38, 3), (42, 5, 39, 4), (43, 5, 40, 5), (44, 5, 40.5, 0), (45, 5, 41, 5), (46, 5, 42, 4), (47, 5, 42.5, 0), (48, 5, 43, 3), (49, 5, 44, 2), (50, 5, 45, 1),
  -- Jordan Spizike Low "White/Black"
  (51, 6, 38, 0), (52, 6, 39, 0), (53, 6, 40, 2), (54, 6, 40.5, 0), (55, 6, 41, 0), (56, 6, 42, 2), (57, 6, 42.5, 0), (58, 6, 43, 1), (59, 6, 44, 0), (60, 6, 45, 0),
  -- Jordan Max Aura 6 "Black/Red"
  (61, 7, 38, 0), (62, 7, 39, 2), (63, 7, 40, 3), (64, 7, 40.5, 0), (65, 7, 41, 3), (66, 7, 42, 4), (67, 7, 42.5, 0), (68, 7, 43, 2), (69, 7, 44, 0), (70, 7, 45, 0),
  -- Jordan Luka 3 "Motorsport"
  (71, 8, 38, 0), (72, 8, 39, 0), (73, 8, 40, 0), (74, 8, 40.5, 0), (75, 8, 41, 3), (76, 8, 42, 4), (77, 8, 42.5, 2), (78, 8, 43, 3), (79, 8, 44, 2), (80, 8, 45, 2),
  -- Jordan Tatum 3 "Zen"
  (81, 9, 38, 0), (82, 9, 39, 0), (83, 9, 40, 2), (84, 9, 40.5, 0), (85, 9, 41, 2), (86, 9, 42, 3), (87, 9, 42.5, 0), (88, 9, 43, 1), (89, 9, 44, 0), (90, 9, 45, 0),
  -- Air Jordan 12 Retro "Taxi"
  (91, 10, 38, 0), (92, 10, 39, 0), (93, 10, 40, 0), (94, 10, 40.5, 0), (95, 10, 41, 0), (96, 10, 42, 0), (97, 10, 42.5, 0), (98, 10, 43, 0), (99, 10, 44, 0), (100, 10, 45, 0);

-- ---------------------------------------------------------------
-- 3) Clientes (el email es único: identifica a la persona en el checkout)
-- ---------------------------------------------------------------
INSERT INTO Clientes (Id, Nombre, Email, Telefono, Direccion, CreadoEn) VALUES
  (1, 'Valentina Ríos', 'valentina.rios@example.com', '11 5555 1234', 'Av. Corrientes 1234, CABA', NOW() - INTERVAL 12 DAY),
  (2, 'Tomás Herrera', 'tomas.herrera@example.com', '11 4444 9876', 'Calle 12 n.º 456, La Plata', NOW() - INTERVAL 5 DAY),
  (3, 'Lucía Fernández', 'lucia.fernandez@example.com', NULL, 'San Martín 789, Rosario', NOW() - INTERVAL 1 DAY);

-- ---------------------------------------------------------------
-- 4) Pedidos: uno por cada estado relevante para poder mostrar el circuito completo
--    Total = SUM(Cantidad * PrecioUnitario) de sus líneas (se verifica en consultas.sql)
-- ---------------------------------------------------------------
INSERT INTO Pedidos (Id, ClienteId, Estado, Total, Nota, CreadoEn, ActualizadoEn) VALUES
  (1, 1, 'entregado', 639998.00, 'Dejar en portería si no hay nadie.', NOW() - INTERVAL 12 DAY, NOW() - INTERVAL 8 DAY),
  (2, 2, 'enviado',   259999.00, NULL,                                  NOW() - INTERVAL 5 DAY,  NOW() - INTERVAL 2 DAY),
  (3, 1, 'cancelado', 449999.00, NULL,                                  NOW() - INTERVAL 3 DAY,  NOW() - INTERVAL 2 DAY),
  (4, 3, 'pendiente', 899997.00, 'Horario de entrega: después de las 18.', NOW() - INTERVAL 1 DAY, NOW() - INTERVAL 1 DAY);

-- ---------------------------------------------------------------
-- 5) Líneas de cada pedido (talle y precio del momento de la compra)
-- ---------------------------------------------------------------
INSERT INTO ItemsPedido (Id, PedidoId, ZapatillaId, Talle, Cantidad, PrecioUnitario) VALUES
  (1, 1, 1, 41, 1, 389999.00),   -- Pedido 1: AJ1 High Chicago talle 41
  (2, 1, 5, 41, 1, 249999.00),   -- Pedido 1: AJ1 Low Bred Toe talle 41
  (3, 2, 8, 43, 1, 259999.00),   -- Pedido 2: Luka 3 talle 43
  (4, 3, 3, 42, 1, 449999.00),   -- Pedido 3 (cancelado): AJ11 Concord talle 42 -> el par volvió al stock
  (5, 4, 2, 42, 1, 419999.00),   -- Pedido 4: AJ4 Bred talle 42
  (6, 4, 9, 42, 2, 239999.00);   -- Pedido 4: Tatum 3 talle 42, dos pares

-- Control rápido
SELECT 'Zapatillas' AS Tabla, COUNT(*) AS Filas FROM Zapatillas
UNION ALL SELECT 'StockTalles', COUNT(*) FROM StockTalles
UNION ALL SELECT 'Clientes', COUNT(*) FROM Clientes
UNION ALL SELECT 'Pedidos', COUNT(*) FROM Pedidos
UNION ALL SELECT 'ItemsPedido', COUNT(*) FROM ItemsPedido;
