const container = document.getElementById("products-table");

const tableUrl = container.dataset.url;
const search = container.dataset.search || "";
const categoryId = container.dataset.categoryId || "";

function buildQuery(page) {
    const query = new URLSearchParams();

    if (search) {
        query.set("search", search);
    }

    if (categoryId) {
        query.set("categoryId", categoryId);
    }

    query.set("page", page);

    return query.toString();
}

function loadPage(page, pushHistory) {
    const query = buildQuery(page);

    container.classList.add("is-loading");

    fetch(tableUrl + "?" + query, { headers: { "X-Requested-With": "XMLHttpRequest" } })
        .then(function (response) {
            if (!response.ok) {
                throw new Error("Request failed with status " + response.status);
            }

            return response.text();
        })
        .then(function (html) {
            container.innerHTML = html;

            if (pushHistory) {
                history.pushState({ page: page }, "", "?" + query);
            }

            container.scrollIntoView({ behavior: "smooth", block: "nearest" });
        })
        .catch(function (err) {
            console.error("Could not load the product page:", err);
        })
        .finally(function () {
            container.classList.remove("is-loading");
        });
}

container.addEventListener("click", function (e) {
    const button = e.target.closest("[data-page]");

    if (!button || button.disabled || button.closest(".page-item").classList.contains("disabled")) {
        return;
    }

    e.preventDefault();

    loadPage(button.dataset.page, true);
});

window.addEventListener("popstate", function (e) {
    const page = (e.state && e.state.page) || new URLSearchParams(location.search).get("page") || 1;

    loadPage(page, false);
});
