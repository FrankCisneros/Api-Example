if (document.body.dataset.page === "home") {
	initializeCatalog();
}

if (document.body.dataset.page === "auth") {
	initializeAuthentication();
}

async function initializeCatalog() {
	const categoryFilters = document.getElementById("categoryFilters");
	const categoryCount = document.getElementById("category-count");
	const productCount = document.getElementById("product-count");
	const productGrid = document.getElementById("productGrid");
	const orderList = document.getElementById("orderList");
	const orderMessage = document.getElementById("orderMessage");
	let products = [];
	let selectedCategory = "all";

	productGrid.addEventListener("click", async (event) => {
		const button = event.target.closest("button[data-product-id]");
		if (!button) return;

		const product = products.find((item) => String(item.id) === button.dataset.productId);
		const quantityInput = button.closest(".product-item").querySelector("[data-order-quantity]");
		const quantity = Number(quantityInput.value);
		if (!product || !Number.isInteger(quantity) || quantity < 1 || quantity > Number(product.stock)) {
			setOrderMessage("Indica una cantidad válida según las existencias.", true);
			return;
		}

		button.disabled = true;
		setOrderMessage("");
		try {
			const response = await fetch("/api/Orders/CreateOrder", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify([{ productId: Number(product.id), quantity }])
			});
			const result = await response.json().catch(() => null);
			if (!response.ok) {
				throw new Error(result?.message ?? result?.title ?? (typeof result === "string" ? result : "No se pudo crear el pedido."));
			}

			setOrderMessage("Pedido creado. Ya puedes consultar su detalle.");
			await loadOrders();
		} catch (error) {
			setOrderMessage(error.message || "No se pudo conectar con el servidor.", true);
		} finally {
			button.disabled = false;
		}
	});

	orderList.addEventListener("click", async (event) => {
		const button = event.target.closest("button[data-order-id]");
		if (!button) return;

		const detailPanel = document.getElementById(`order-detail-${button.dataset.orderId}`);
		if (!detailPanel) return;
		if (button.getAttribute("aria-expanded") === "true") {
			detailPanel.hidden = true;
			button.setAttribute("aria-expanded", "false");
			button.textContent = "Ver detalle";
			return;
		}

		button.disabled = true;
		try {
			const order = await fetchJson(`/api/Orders/Orders/${button.dataset.orderId}`);
			renderOrderDetails(detailPanel, order);
			detailPanel.hidden = false;
			button.setAttribute("aria-expanded", "true");
			button.textContent = "Ocultar detalle";
		} catch {
			setOrderMessage("No se pudo cargar el detalle del pedido.", true);
		} finally {
			button.disabled = false;
		}
	});

	function setOrderMessage(message, isError = false) {
		orderMessage.textContent = message;
		orderMessage.classList.toggle("is-error", isError);
	}

	async function loadOrders() {
		orderList.setAttribute("aria-busy", "true");
		try {
			const pageSize = 100;
			let response = await fetchJson(`/api/Orders/Orders?page=1&size=${pageSize}`);
			const total = Number(response.total ?? response.Total ?? 0);
			const lastPage = Math.max(1, Math.ceil(total / pageSize));
			if (lastPage > 1) {
				response = await fetchJson(`/api/Orders/Orders?page=${lastPage}&size=${pageSize}`);
			}
			const orders = [...(response.Data ?? response.data ?? [])].reverse();
			orderList.setAttribute("aria-busy", "false");
			if (orders.length === 0) {
				orderList.innerHTML = '<p class="empty-message">Todavía no hay pedidos.</p>';
				return;
			}

			orderList.innerHTML = orders.map((order) => {
				const details = order.orderDetails ?? order.OrderDetails ?? [];
				const productNames = details.map((detail) => detail.product?.name ?? `Producto #${detail.productId}`).join(", ");
				const orderDate = new Date(order.orderDate);
				const dateLabel = Number.isNaN(orderDate.getTime()) ? "Fecha no disponible" : orderDate.toLocaleString("es-ES");
				const productLabel = productNames || `Producto #${order.productId}`;
				const quantity = details.reduce((total, detail) => total + Number(detail.quantity || 0), 0) || Number(order.quantity || 0);
				const orderId = escapeHtml(String(order.id));

				return `
					<article class="order-item">
						<div class="order-item-summary">
							<p class="order-meta">Pedido #${orderId} <span>${escapeHtml(dateLabel)}</span></p>
							<h3>${escapeHtml(productLabel)}</h3>
							<p class="order-quantity">${quantity} ${quantity === 1 ? "unidad" : "unidades"}</p>
						</div>
						<button class="button button-outline order-detail-button" type="button" data-order-id="${orderId}" aria-expanded="false" aria-controls="order-detail-${orderId}">Ver detalle</button>
						<div id="order-detail-${orderId}" class="order-detail-panel" hidden></div>
					</article>
				`;
			}).join("");
		} catch {
			orderList.setAttribute("aria-busy", "false");
			orderList.innerHTML = '<p class="empty-message">No se pudieron cargar los pedidos.</p>';
		}
	}

	void loadOrders();

	try {
		const [productResponse, categories] = await Promise.all([
			fetchJson("/api/Product?page=1&size=100"),
			fetchJson("/api/Product/Categories")
		]);

		products = Array.isArray(productResponse)
			? productResponse
			: productResponse.Data ?? productResponse.data ?? [];

		const categoryNames = new Map(
			categories.map((category) => [category.id, category.name])
		);

		renderCategories(categories);
		renderProducts();

		function renderCategories(categoryList) {
			categoryCount.textContent = `${categoryList.length}`;
			categoryFilters.replaceChildren();

			const filters = [
				{ id: "all", name: "Todos" },
				...categoryList.map((category) => ({
					id: String(category.id),
					name: category.name
				}))
			];

			for (const category of filters) {
				const button = document.createElement("button");
				button.type = "button";
				button.className = "category-filter";
				button.textContent = category.name;
				button.setAttribute("aria-pressed", String(category.id === selectedCategory));
				button.addEventListener("click", () => {
					selectedCategory = category.id;
					for (const filter of categoryFilters.querySelectorAll("button")) {
						filter.setAttribute("aria-pressed", String(filter === button));
					}
					renderProducts();
				});
				categoryFilters.append(button);
			}
		}

		function renderProducts() {
			const visibleProducts = selectedCategory === "all"
				? products
				: products.filter((product) => String(product.categoryId) === selectedCategory);

			productCount.textContent = `${visibleProducts.length} ${visibleProducts.length === 1 ? "producto" : "productos"}`;
			productGrid.setAttribute("aria-busy", "false");

			if (visibleProducts.length === 0) {
				productGrid.innerHTML = '<p class="empty-message">No hay productos en esta categoría todavía.</p>';
				return;
			}

			productGrid.innerHTML = visibleProducts.map((product) => {
				const categoryName = categoryNames.get(product.categoryId) ?? "Sin categoría";
				const price = new Intl.NumberFormat("es-ES", {
					style: "currency",
					currency: "EUR"
				}).format(product.price);
				const stock = Number(product.stock) || 0;
				const stockClass = stock > 0 ? "in-stock" : "out-of-stock";
				const stockLabel = stock > 0 ? `${stock} disponibles` : "Agotado";
				const productId = escapeHtml(String(product.id));

				return `
					<article class="product-item">
						<div class="product-item-top">
							<span class="product-category">${escapeHtml(categoryName)}</span>
							<span class="product-stock ${stockClass}">${stockLabel}</span>
						</div>
						<h3>${escapeHtml(product.name)}</h3>
						<p class="product-description">${escapeHtml(product.description ?? "")}</p>
						<div class="product-item-bottom">
							<span class="product-price">${price}</span>
							<span class="product-id">#${productId}</span>
						</div>
						<div class="product-order-controls">
							<label for="quantity-${productId}">Cantidad</label>
							<input id="quantity-${productId}" data-order-quantity type="number" min="1" max="${stock}" value="1" inputmode="numeric" ${stock === 0 ? "disabled" : ""}>
							<button class="button button-dark create-order-button" type="button" data-product-id="${productId}" ${stock === 0 ? "disabled" : ""}>Pedir <span aria-hidden="true">→</span></button>
						</div>
					</article>
				`;
			}).join("");
		}
	} catch {
		categoryFilters.innerHTML = '<p class="empty-message">No se pudieron cargar las categorías.</p>';
		productGrid.setAttribute("aria-busy", "false");
		productGrid.innerHTML = '<p class="empty-message">No se pudo conectar con el servidor. Inténtalo de nuevo más tarde.</p>';
	}

	function renderOrderDetails(panel, order) {
		const details = order.orderDetails ?? order.OrderDetails ?? [];
		if (details.length === 0) {
			panel.innerHTML = '<p class="empty-message">Este pedido no tiene líneas de detalle.</p>';
			return;
		}

		panel.innerHTML = details.map((detail) => {
			const product = detail.product ?? detail.Product;
			const productName = product?.name ?? `Producto #${detail.productId}`;
			const quantity = Number(detail.quantity || 0);
			const lineTotal = product?.price == null
				? ""
				: formatCurrency(Number(product.price) * quantity);
			return `
				<div class="order-detail-line">
					<span><strong>${escapeHtml(productName)}</strong><small>Producto #${escapeHtml(String(detail.productId))} · Cantidad: ${quantity}</small></span>
					${lineTotal ? `<span class="order-line-total">${lineTotal}</span>` : ""}
				</div>
			`;
		}).join("");
	}
}

async function fetchJson(url) {
	const response = await fetch(url);
	if (!response.ok) {
		throw new Error(`La solicitud falló: ${response.status}`);
	}
	return response.json();
}

function escapeHtml(value) {
	return String(value).replace(/[&<>"']/g, (character) => ({
		"&": "&amp;",
		"<": "&lt;",
		">": "&gt;",
		'"': "&quot;",
		"'": "&#39;"
	})[character]);
}

function initializeAuthentication() {
	const loginSection = document.getElementById("loginSection");
	const registerSection = document.getElementById("registerSection");
	const loginTab = document.getElementById("loginTab");
	const registerTab = document.getElementById("registerTab");
	const loginForm = document.getElementById("loginForm");
	const registerForm = document.getElementById("registerForm");
	const authMessage = document.getElementById("authMessage");

	function setAuthMode(mode) {
		const showLogin = mode === "login";
		loginSection.hidden = !showLogin;
		registerSection.hidden = showLogin;
		loginTab.classList.toggle("is-active", showLogin);
		registerTab.classList.toggle("is-active", !showLogin);
		loginTab.setAttribute("aria-selected", String(showLogin));
		registerTab.setAttribute("aria-selected", String(!showLogin));
		authMessage.textContent = "";
	}

	loginTab.addEventListener("click", () => setAuthMode("login"));
	registerTab.addEventListener("click", () => setAuthMode("register"));

	loginForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		await submitAuthForm("login", {
			email: document.getElementById("email").value,
			password: document.getElementById("password").value
		});
	});

	registerForm.addEventListener("submit", async (event) => {
		event.preventDefault();
		await submitAuthForm("register", {
			name: document.getElementById("registerName").value,
			email: document.getElementById("registerEmail").value,
			password: document.getElementById("registerPassword").value,
			number: Number(document.getElementById("registerNumber").value)
		});
	});

	async function submitAuthForm(action, data) {
		authMessage.textContent = "";

		try {
			const response = await fetch(`/api/UserLogin/${action}`, {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify(data)
			});
			const result = await response.json().catch(() => null);

			if (!response.ok) {
				authMessage.textContent = result?.message ?? result?.title ?? "No se pudo completar la operación.";
				authMessage.classList.add("is-error");
				return;
			}

			authMessage.classList.remove("is-error");
			authMessage.textContent = result?.message ?? "Operación completada.";
			if (action === "login") {
				window.location.href = "/Home.html";
				return;
			}

			document.getElementById("email").value = data.email;
			registerForm.reset();
			setAuthMode("login");
			authMessage.textContent = "Cuenta creada. Ya puedes iniciar sesión.";
		} catch {
			authMessage.textContent = "No se pudo conectar con el servidor.";
			authMessage.classList.add("is-error");
		}
	}
}

function formatCurrency(value) {
	return new Intl.NumberFormat("es-ES", { style: "currency", currency: "EUR" }).format(value);
}