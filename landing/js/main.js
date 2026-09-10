// URL base del backend en C#/ASP.NET Core. Cambiala si tu API corre en otro puerto/dominio.
const API_URL = 'http://localhost:5000/api';

const gridPizzas = document.getElementById('grid-pizzas');
const estadoCarga = document.getElementById('estado-carga');
const filtros = document.getElementById('filtros');
const listaCarrito = document.getElementById('lista-carrito');
const totalCarritoEl = document.getElementById('total-carrito');
const carritoVacio = document.getElementById('carrito-vacio');
const formPedido = document.getElementById('form-pedido');
const mensajePedido = document.getElementById('mensaje-pedido');

let carrito = []; // [{ pizza_id, nombre, precio, cantidad }]

// --- Traer pizzas desde la API ---
async function cargarPizzas(categoria = '') {
  estadoCarga.hidden = false;
  estadoCarga.textContent = 'Cargando pizzas desde la API...';
  gridPizzas.innerHTML = '';

  try {
    const url = categoria ? `${API_URL}/pizzas?categoria=${categoria}` : `${API_URL}/pizzas`;
    const res = await fetch(url);
    if (!res.ok) throw new Error('La API respondio con un error');
    const pizzas = await res.json();

    estadoCarga.hidden = true;
    if (pizzas.length === 0) {
      estadoCarga.hidden = false;
      estadoCarga.textContent = 'No hay pizzas en esta categoria.';
      return;
    }
    pizzas.forEach(renderPizza);
  } catch (err) {
    estadoCarga.hidden = false;
    estadoCarga.textContent =
      'No se pudo conectar con la API (' + err.message + '). ¿Esta corriendo el backend en el puerto 5000?';
  }
}

function renderPizza(pizza) {
  const card = document.createElement('div');
  card.className = 'card-pizza';
  card.innerHTML = `
    <span class="categoria">${pizza.categoria}</span>
    <h3>${pizza.nombre}</h3>
    <p class="desc">${pizza.descripcion || ''}</p>
    <div class="precio">$${pizza.precio}</div>
    <button data-id="${pizza.id}">Agregar al pedido</button>
  `;
  card.querySelector('button').addEventListener('click', () => agregarAlCarrito(pizza));
  gridPizzas.appendChild(card);
}

// --- Filtros de categoria ---
filtros.addEventListener('click', (e) => {
  if (!e.target.matches('.filtro')) return;
  filtros.querySelectorAll('.filtro').forEach((b) => b.classList.remove('activo'));
  e.target.classList.add('activo');
  cargarPizzas(e.target.dataset.categoria);
});

// --- Carrito ---
function agregarAlCarrito(pizza) {
  const existente = carrito.find((i) => i.pizza_id === pizza.id);
  if (existente) {
    existente.cantidad += 1;
  } else {
    carrito.push({ pizza_id: pizza.id, nombre: pizza.nombre, precio: pizza.precio, cantidad: 1 });
  }
  renderCarrito();
}

function quitarDelCarrito(pizzaId) {
  carrito = carrito.filter((i) => i.pizza_id !== pizzaId);
  renderCarrito();
}

function renderCarrito() {
  listaCarrito.innerHTML = '';
  if (carrito.length === 0) {
    carritoVacio.hidden = false;
    totalCarritoEl.textContent = '';
    formPedido.hidden = true;
    return;
  }
  carritoVacio.hidden = true;
  formPedido.hidden = false;

  let total = 0;
  carrito.forEach((item) => {
    total += item.precio * item.cantidad;
    const li = document.createElement('li');
    li.innerHTML = `
      <span>${item.cantidad} x ${item.nombre} — $${item.precio * item.cantidad}</span>
      <button data-id="${item.pizza_id}">Quitar</button>
    `;
    li.querySelector('button').addEventListener('click', () => quitarDelCarrito(item.pizza_id));
    listaCarrito.appendChild(li);
  });
  totalCarritoEl.textContent = `Total: $${total}`;
}

// --- Enviar pedido a la API ---
formPedido.addEventListener('submit', async (e) => {
  e.preventDefault();
  mensajePedido.textContent = '';
  mensajePedido.className = 'mensaje-pedido';

  const payload = {
    cliente: {
      nombre: document.getElementById('nombre').value,
      email: document.getElementById('email').value,
      telefono: document.getElementById('telefono').value,
      direccion: document.getElementById('direccion').value,
    },
    items: carrito.map((i) => ({ pizza_id: i.pizza_id, cantidad: i.cantidad })),
  };

  try {
    const res = await fetch(`${API_URL}/pedidos`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      const err = await res.json();
      throw new Error(err.error || 'Error al crear el pedido');
    }
    const pedido = await res.json();
    mensajePedido.textContent = `¡Pedido #${pedido.id} confirmado! Total: $${pedido.total}`;
    mensajePedido.classList.add('ok');
    carrito = [];
    renderCarrito();
    formPedido.reset();
  } catch (err) {
    mensajePedido.textContent = 'Error: ' + err.message;
    mensajePedido.classList.add('error');
  }
});

// Carga inicial
cargarPizzas();
renderCarrito();
