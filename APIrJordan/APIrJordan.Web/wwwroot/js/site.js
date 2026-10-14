/* APIr Jordan — interacciones livianas, sin librerías.
   Todo lo importante funciona sin JavaScript: esto solo suma animaciones y comodidad. */
(function () {
    "use strict";

    var reducirMovimiento = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var formatoNumero = new Intl.NumberFormat("es-AR");

    document.addEventListener("DOMContentLoaded", function () {
        revelarAlHacerScroll();
        avisos();
        selectorDeTalle();
        steppers();
        contadores();
        barras();
        lineasDeTiempo();
        confirmaciones();
        vistaPrevia();
        totalDeStock();
    });

    /* Aparición en cascada de tarjetas y secciones (.revelar) */
    function revelarAlHacerScroll() {
        var elementos = document.querySelectorAll(".revelar");
        if (!elementos.length) return;

        if (reducirMovimiento || !("IntersectionObserver" in window)) {
            elementos.forEach(function (el) { el.classList.add("visible"); });
            return;
        }

        var observador = new IntersectionObserver(function (entradas) {
            entradas.forEach(function (entrada) {
                if (entrada.isIntersecting) {
                    entrada.target.classList.add("visible");
                    observador.unobserve(entrada.target);
                }
            });
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });

        elementos.forEach(function (el, i) {
            if (!el.style.getPropertyValue("--i")) el.style.setProperty("--i", String(i % 8));
            observador.observe(el);
        });
    }

    /* Avisos (toasts): se cierran solos y con el botón; el contador del carrito late si hubo novedades */
    function avisos() {
        var lista = document.querySelectorAll("[data-aviso]");
        if (!lista.length) return;

        var contador = document.querySelector("[data-contador-carrito]");
        if (contador && !contador.classList.contains("vacio")) contador.classList.add("latido");

        lista.forEach(function (aviso) {
            var cerrar = function () {
                aviso.classList.add("ocultar");
                setTimeout(function () { aviso.remove(); }, 350);
            };
            var boton = aviso.querySelector("[data-cerrar-aviso]");
            if (boton) boton.addEventListener("click", cerrar);
            setTimeout(cerrar, 4500);
        });
    }

    /* Ficha: al elegir un talle se habilita el botón y se muestra cuántos pares quedan */
    function selectorDeTalle() {
        var grupo = document.querySelector("[data-talles]");
        if (!grupo) return;

        var ayuda = document.querySelector("[data-talle-ayuda]");
        var boton = document.querySelector("[data-boton-comprar]");
        var cantidad = document.querySelector("[data-cantidad-compra]");

        var actualizar = function () {
            var elegido = grupo.querySelector("input[type=radio]:checked");
            if (!elegido) {
                if (boton) boton.classList.add("deshabilitado");
                if (ayuda) ayuda.textContent = "Elegí un talle para continuar.";
                return;
            }
            var stock = parseInt(elegido.getAttribute("data-stock"), 10) || 0;
            if (boton) boton.classList.remove("deshabilitado");
            if (ayuda) {
                ayuda.textContent = stock === 1
                    ? "Talle " + elegido.value + " · queda el último par."
                    : "Talle " + elegido.value + " · quedan " + stock + " pares.";
            }
            if (cantidad) {
                cantidad.max = String(Math.min(stock, parseInt(cantidad.getAttribute("data-maximo"), 10) || 5));
                if (parseInt(cantidad.value, 10) > parseInt(cantidad.max, 10)) cantidad.value = cantidad.max;
            }
        };

        grupo.querySelectorAll("input[type=radio]").forEach(function (radio) {
            radio.addEventListener("change", actualizar);
        });
        actualizar();
    }

    /* Botones +/- de cantidad (.stepper) */
    function steppers() {
        document.querySelectorAll("[data-stepper]").forEach(function (boton) {
            boton.addEventListener("click", function () {
                var contenedor = boton.closest(".stepper");
                var input = contenedor && contenedor.querySelector("input");
                if (!input) return;

                var paso = boton.getAttribute("data-stepper") === "+" ? 1 : -1;
                var minimo = parseInt(input.min || "0", 10);
                var maximo = parseInt(input.max || "99", 10);
                var valor = (parseInt(input.value, 10) || 0) + paso;

                input.value = String(Math.max(minimo, Math.min(maximo, valor)));
                input.dispatchEvent(new Event("change", { bubbles: true }));

                // En el carrito, cambiar la cantidad envía el formulario directamente
                if (contenedor.hasAttribute("data-enviar")) {
                    var formulario = boton.closest("form");
                    if (formulario) formulario.requestSubmit ? formulario.requestSubmit() : formulario.submit();
                }
            });
        });
    }

    /* Números del panel que suben desde cero */
    function contadores() {
        var elementos = document.querySelectorAll("[data-contador]");
        if (!elementos.length) return;

        elementos.forEach(function (el) {
            var fin = parseFloat(el.getAttribute("data-contador")) || 0;
            var prefijo = el.getAttribute("data-prefijo") || "";

            if (reducirMovimiento) { el.textContent = prefijo + formatoNumero.format(fin); return; }

            var inicio = null;
            var duracion = 900;
            var paso = function (tiempo) {
                if (inicio === null) inicio = tiempo;
                var progreso = Math.min(1, (tiempo - inicio) / duracion);
                var suavizado = 1 - Math.pow(1 - progreso, 3);
                el.textContent = prefijo + formatoNumero.format(Math.round(fin * suavizado));
                if (progreso < 1) requestAnimationFrame(paso);
            };
            requestAnimationFrame(paso);
        });
    }

    /* Barras de los rankings: el ancho real se aplica después de pintar, para que se vea la transición */
    function barras() {
        var rellenos = document.querySelectorAll("[data-porcentaje]");
        if (!rellenos.length) return;
        requestAnimationFrame(function () {
            setTimeout(function () {
                rellenos.forEach(function (el) { el.style.width = el.getAttribute("data-porcentaje") + "%"; });
            }, 80);
        });
    }

    /* Línea de tiempo del pedido: el progreso se dibuja con una transición */
    function lineasDeTiempo() {
        document.querySelectorAll("[data-progreso]").forEach(function (el) {
            setTimeout(function () { el.style.setProperty("--progreso", el.getAttribute("data-progreso")); }, 120);
        });
    }

    /* Confirmación antes de acciones destructivas (eliminar, cancelar pedido, vaciar carrito) */
    function confirmaciones() {
        document.querySelectorAll("form[data-confirmar]").forEach(function (formulario) {
            formulario.addEventListener("submit", function (evento) {
                if (!window.confirm(formulario.getAttribute("data-confirmar"))) evento.preventDefault();
            });
        });
    }

    /* Formulario de modelo: al cambiar la ilustración se actualiza la vista previa */
    function vistaPrevia() {
        var selector = document.querySelector("[data-vista-previa]");
        var imagen = document.querySelector("[data-imagen-previa]");
        if (!selector || !imagen) return;

        var base = imagen.getAttribute("data-base") || "";
        var generica = imagen.getAttribute("data-generica") || "";

        selector.addEventListener("change", function () {
            imagen.style.opacity = "0";
            setTimeout(function () {
                imagen.src = selector.value ? base + selector.value : generica;
                imagen.style.opacity = "1";
            }, 180);
        });
    }

    /* Formulario de modelo: suma en vivo los pares cargados por talle */
    function totalDeStock() {
        var celdas = document.querySelectorAll("[data-stock-talle]");
        var salida = document.querySelector("[data-stock-total]");
        if (!celdas.length || !salida) return;

        var recalcular = function () {
            var total = 0;
            celdas.forEach(function (input) { total += parseInt(input.value, 10) || 0; });
            salida.textContent = total === 1 ? "1 par" : total + " pares";
        };
        celdas.forEach(function (input) { input.addEventListener("input", recalcular); });
        recalcular();
    }
})();
