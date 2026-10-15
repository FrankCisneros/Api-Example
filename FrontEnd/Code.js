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
	let products = [];
	let selectedCategory = "all";

	try {
		const [productResponse, categories] = await Promise.all([
			fetchJson("/api/Product"),
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
							<span class="product-id">#${escapeHtml(String(product.id))}</span>
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