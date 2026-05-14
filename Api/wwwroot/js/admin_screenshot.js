// admin.js

const API_BASE = window.location.origin;
const authToken = localStorage.getItem('jwt');

// Redireciona se não for admin logado
if (!authToken || localStorage.getItem('jwt_role') !== 'admin') {
    window.location.href = '/index.html';
}

// Global Data
let allOrders = [];
let allProducts = [];
let allCategories = [];

const statusConfig = {
    'Concluído': { class: 'status-success', bg: 'var(--status-success-bg)', color: 'var(--status-success)' },
    'Pago': { class: 'status-paid', bg: 'var(--status-paid-bg)', color: 'var(--status-paid)' },
    'Aguardando': { class: 'status-waiting', bg: 'var(--status-waiting-bg)', color: 'var(--status-waiting)' },
    'Pendente': { class: 'status-waiting', bg: 'var(--status-waiting-bg)', color: 'var(--status-waiting)' },
    'Preparando': { class: 'status-prep', bg: 'var(--status-prep-bg)', color: 'var(--status-prep)' },
    'Cancelado': { class: 'status-cancel', bg: 'var(--status-cancel-bg)', color: 'var(--status-cancel)' }
};

// Imagens default (mesmo padrão do app.js)
const FOOD_IMAGES = {
    'smash': 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'classico': 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'bbq': 'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=500&h=350&fit=crop',
    'bacon': 'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=500&h=350&fit=crop',
    'trufa': 'https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=500&h=350&fit=crop',
    'supreme': 'https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=500&h=350&fit=crop',
    'hambur': 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'burger': 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'lanche': 'https://images.unsplash.com/photo-1550547660-d9450f859349?w=500&h=350&fit=crop',
    'batata': 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'rustica': 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'frita': 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'onion': 'https://images.unsplash.com/photo-1541592106381-b31e9677c0e5?w=500&h=350&fit=crop',
    'refri': 'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&h=350&fit=crop',
    'coca': 'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&h=350&fit=crop',
    'milkshake': 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?w=500&h=350&fit=crop',
    'sorvete': 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?w=500&h=350&fit=crop',
    'suco': 'https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=500&h=350&fit=crop',
    'default': 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=500&h=350&fit=crop'
};

function obterImagemProduto(produto) {
    if (typeof produto === 'object' && produto.imageUrl && produto.imageUrl.startsWith('http')) return produto.imageUrl;
    const nome = typeof produto === 'string' ? produto : produto?.nome;
    if (!nome) return FOOD_IMAGES['default'];
    const nomeLower = nome.toLowerCase();
    for (const [chave, url] of Object.entries(FOOD_IMAGES)) {
        if (chave !== 'default' && nomeLower.includes(chave)) return url;
    }
    return FOOD_IMAGES['default'];
}

// -- Initialization --
document.addEventListener('DOMContentLoaded', async () => {
    // Initialize Lucide icons
    lucide.createIcons();

    // Set Date & User
    const dateOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    document.getElementById('currentDate').textContent = new Date().toLocaleDateString('pt-BR', dateOptions);

    // Setup Navigation
    setupNavigation();
    setupMobileMenu();
    setupOrderFilters();

    // Fetch API Data
    await carregarDadosIniciais();
});

async function carregarDadosIniciais() {
    try {
        const headers = { 'Authorization': `Bearer ${authToken}` };

        // Fetch Pedidos
        const resPedidos = await fetch(`${API_BASE}/api/Pedido`, { headers });
        if (resPedidos.ok) allOrders = await resPedidos.json();

        // Fetch Produtos
        const resProdutos = await fetch(`${API_BASE}/api/Produtos`, { headers });
        if (resProdutos.ok) allProducts = await resProdutos.json();

        // Fetch Categorias
        const resCat = await fetch(`${API_BASE}/api/Categorias`, { headers });
        if (resCat.ok) allCategories = await resCat.json();

        renderDashboard();
        renderOrders('Todos');
        renderProducts();
        renderCategories();

    } catch (err) {
        console.error("Erro ao carregar dados", err);
    }
}

// -- Navigation Logic --
function setupNavigation() {
    const navItems = document.querySelectorAll('.sidebar-nav .nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            const target = item.getAttribute('data-target');
            if (target) {
                e.preventDefault();
                switchTab(target);
            }
        });
    });
}

window.switchTab = function (targetId) {
    document.querySelectorAll('.sidebar-nav .nav-item').forEach(el => el.classList.remove('active'));
    const targetLink = document.querySelector(`.sidebar-nav .nav-item[data-target="${targetId}"]`);
    if (targetLink) targetLink.classList.add('active');

    document.querySelectorAll('.view').forEach(el => el.classList.remove('active'));
    const targetView = document.getElementById(`view-${targetId}`);
    if (targetView) targetView.classList.add('active');

    closeMobileMenu();
}

function setupMobileMenu() {
    const toggleBtn = document.getElementById('menuToggleBtn');
    const closeBtn = document.getElementById('closeMenuBtn');
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    function openMobileMenu() {
        sidebar.classList.add('open');
        overlay.style.display = 'block';
    }

    window.closeMobileMenu = function () {
        sidebar.classList.remove('open');
        overlay.style.display = 'none';
    }

    if (toggleBtn) toggleBtn.addEventListener('click', openMobileMenu);
    if (closeBtn) closeBtn.addEventListener('click', closeMobileMenu);
    if (overlay) overlay.addEventListener('click', closeMobileMenu);
}

// -- Dashboard Rendering --
function renderDashboard() {
    const hojeStr = new Date().toDateString();

    // Calcula KPIs
    let receitaHoje = 0;
    let pedidosHoje = 0;

    allOrders.forEach(o => {
        const orderDate = new Date(o.dataPedido).toDateString();
        if (orderDate === hojeStr) {
            pedidosHoje++;
            // Apenas pedidos que não foram cancelados contam para receita
            if (!o.status.startsWith('Cancelado')) {
                receitaHoje += o.valorTotal;
            }
        }
    });

    const ticketMedio = pedidosHoje > 0 ? (receitaHoje / pedidosHoje) : 0;

    // Atualiza KPIs no DOM
    document.querySelector('.kpi-card:nth-child(1) .kpi-value').textContent = `R$ ${receitaHoje.toFixed(2).replace('.', ',')}`;
    document.querySelector('.kpi-card:nth-child(2) .kpi-value').textContent = pedidosHoje;
    document.querySelector('.kpi-card:nth-child(3) .kpi-value').textContent = `R$ ${ticketMedio.toFixed(2).replace('.', ',')}`;

    // Chart.js - Faturamento 7 dias
    const ctx = document.getElementById('revenueChart').getContext('2d');

    // Gerar últimos 7 dias
    const labels = [];
    const chartData = [];
    for (let i = 6; i >= 0; i--) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        labels.push(d.toLocaleDateString('pt-BR', { weekday: 'short' }));

        // Sum revenue for this day
        const dayStr = d.toDateString();
        const dailyRev = allOrders.filter(o => !o.status.startsWith('Cancelado') && new Date(o.dataPedido).toDateString() === dayStr)
            .reduce((sum, o) => sum + o.valorTotal, 0);
        chartData.push(dailyRev);
    }

    // Se já existe um gráfico, destruí-lo antes de recriar
    if (window.revenueChartInstance) {
        window.revenueChartInstance.destroy();
    }

    window.revenueChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Faturamento (R$)',
                data: chartData,
                borderColor: '#FF8C00',
                backgroundColor: 'rgba(255, 140, 0, 0.1)',
                borderWidth: 3,
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#FF8C00',
                pointBorderColor: '#1a1a2a',
                pointBorderWidth: 2,
                pointRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, grid: { color: 'rgba(255,255,255,0.05)', borderDash: [5, 5] }, ticks: { color: '#8b8ba3' } },
                x: { grid: { display: false }, ticks: { color: '#8b8ba3' } }
            }
        }
    });

    // Latest Orders
    const latestContainer = document.getElementById('latestOrdersContainer');
    const sortedOrders = [...allOrders].sort((a, b) => new Date(b.dataPedido) - new Date(a.dataPedido));

    if (sortedOrders.length === 0) {
        latestContainer.innerHTML = '<p style="color:var(--text-muted); font-size:0.85rem">Nenhum pedido recente.</p>';
    } else {
        latestContainer.innerHTML = sortedOrders.slice(0, 4).map(o => {
            const time = new Date(o.dataPedido).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
            const initials = o.nomeCliente.substring(0, 2).toUpperCase();
            const statusKey = Object.keys(statusConfig).find(k => o.status.includes(k)) || 'Aguardando';
            return `
                <div class="simple-order-item">
                    <div class="order-user-info">
                        <img src="https://ui-avatars.com/api/?name=${initials}&background=22223a&color=f0f0f5" alt="${o.nomeCliente}">
                        <div class="order-details">
                            <h4>${o.nomeCliente}</h4>
                            <p>#${o.pedidoId} • ${time}</p>
                        </div>
                    </div>
                    <div class="order-meta">
                        ${getBadgeHtml(statusKey, o.status)}
                        <span class="order-val">R$ ${o.valorTotal.toFixed(2).replace('.', ',')}</span>
                    </div>
                </div>
            `;
        }).join('');
    }

    // Pipeline
    const pipelineCounts = { 'Pendente': 0, 'Pago': 0, 'Preparando': 0, 'Concluído': 0, 'Cancelado': 0 };
    allOrders.forEach(o => {
        const key = Object.keys(pipelineCounts).find(k => o.status.includes(k)) || 'Pendente';
        pipelineCounts[key]++;
    });

    const pipeData = [
        { label: 'Pendente', count: pipelineCounts['Pendente'], color: 'var(--status-waiting)' },
        { label: 'Pagos', count: pipelineCounts['Pago'], color: 'var(--status-paid)' },
        { label: 'Preparando', count: pipelineCounts['Preparando'], color: 'var(--status-prep)' },
        { label: 'Concluídos', count: pipelineCounts['Concluído'], color: 'var(--status-success)' }
    ];

    const totalPipe = pipeData.reduce((acc, curr) => acc + curr.count, 0) || 1; // avoid / 0
    document.getElementById('pipelineContainer').innerHTML = pipeData.map(item => `
        <div class="pipeline-item">
            <div class="pipe-header">
                <span>${item.label}</span>
                <span>${item.count}</span>
            </div>
            <div class="pipe-bar-bg">
                <div class="pipe-bar-fill" style="width: ${(item.count / totalPipe) * 100}%; background-color: ${item.color}"></div>
            </div>
        </div>
    `).join('');

    // Top Sellers
    const itemCounts = {};
    allOrders.filter(o => !o.status.startsWith('Cancelado')).forEach(o => {
        if (o.itens) {
            o.itens.forEach(i => {
                if (!itemCounts[i.nomeProduto]) itemCounts[i.nomeProduto] = 0;
                itemCounts[i.nomeProduto] += i.quantidade;
            });
        }
    });

    const topSellers = Object.entries(itemCounts)
        .map(([name, qty]) => ({ name, qty }))
        .sort((a, b) => b.qty - a.qty)
        .slice(0, 3);

    const sellersContainer = document.getElementById('topSellersContainer');
    if (topSellers.length === 0) {
        sellersContainer.innerHTML = '<p style="color:var(--text-muted); font-size:0.85rem">Nenhuma venda ainda.</p>';
    } else {
        sellersContainer.innerHTML = topSellers.map((item, idx) => `
            <div class="top-seller-item">
                <div class="seller-info">
                    <div class="rank-badge top-${idx + 1}">${idx + 1}</div>
                    <div class="seller-details">
                        <h4>${item.name}</h4>
                    </div>
                </div>
                <span class="seller-qty">${item.qty} un.</span>
            </div>
        `).join('');
    }
}

// -- Orders Rendering --
function renderOrders(filter) {
    const container = document.getElementById('ordersListContainer');
    const sortedOrders = [...allOrders].sort((a, b) => new Date(b.dataPedido) - new Date(a.dataPedido));

    const filteredOrders = filter === 'Todos'
        ? sortedOrders
        : sortedOrders.filter(o => {
            if (filter === 'Cancelado') return o.status.startsWith('Cancelado');
            if (filter === 'Aguardando') return o.status === 'Pendente';
            return o.status === filter;
        });

    if (filteredOrders.length === 0) {
        container.innerHTML = '<p class="subtitle" style="grid-column: 1/-1; text-align: center; padding: 2rem;">Nenhum pedido encontrado.</p>';
        return;
    }

    container.innerHTML = filteredOrders.map(order => {
        const statusKey = Object.keys(statusConfig).find(k => order.status.includes(k)) || 'Pendente';
        const dateObj = new Date(order.dataPedido);
        const dataFormatada = dateObj.toLocaleDateString('pt-BR') + ' ' + dateObj.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });

        let qtdItens = 0;
        let itensHtml = '';
        if (order.itens) {
            qtdItens = order.itens.reduce((acc, curr) => acc + curr.quantidade, 0);
            itensHtml = order.itens.map(i => `${i.quantidade}x ${i.nomeProduto}`).join(', ');
        }

        return `
        <div class="order-card" style="${statusKey === 'Cancelado' ? 'opacity: 0.6;' : ''}">
            <div class="order-card-header">
                <div>
                    <span class="order-id">#${order.pedidoId}</span>
                    <span class="order-date">${dataFormatada}</span>
                </div>
                ${getBadgeHtml(statusKey, order.status)}
            </div>
            <div class="order-body">
                <strong style="color:var(--text-main)">Cliente:</strong> ${order.nomeCliente} <br>
                <strong style="color:var(--text-main)">Itens (${qtdItens}):</strong> <span title="${itensHtml}">${itensHtml.length > 50 ? itensHtml.substring(0, 50) + '...' : itensHtml || 'Nenhum'}</span>
            </div>
            <div class="order-footer">
                <span class="order-total" style="${statusKey === 'Cancelado' ? 'text-decoration: line-through;' : ''}">R$ ${order.valorTotal.toFixed(2).replace('.', ',')}</span>
                <div class="order-actions">
                    ${!order.status.startsWith('Cancelado') ? `<button class="btn btn-outline" style="color:var(--status-cancel); border-color:var(--status-cancel-bg)" onclick="cancelarPedidoAdmin(${order.pedidoId})">Cancelar</button>` : ''}
                    <button class="btn btn-dark" onclick="abrirOrderModal(${order.pedidoId})">Detalhes</button>
                </div>
            </div>
        </div>
        `;
    }).join('');
}

window.cancelarPedidoAdmin = async function (id) {
    if (!confirm(`Tem certeza que deseja cancelar o pedido #${id}?`)) return;
    try {
        const response = await fetch(`${API_BASE}/api/Pedido/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        if (response.ok) {
            await carregarDadosIniciais();
        } else {
            alert("Erro ao cancelar o pedido.");
        }
    } catch (e) { alert('Erro de conexão'); }
}

function setupOrderFilters() {
    const btns = document.querySelectorAll('.filters-bar .pill-btn');
    btns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            btns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            renderOrders(btn.getAttribute('data-filter'));
        });
    });
}

// -- Products Rendering --
function renderProducts() {
    const container = document.getElementById('productsListContainer');

    if (allProducts.length === 0) {
        container.innerHTML = '<p class="subtitle" style="text-align: center; padding: 2rem;">Nenhum produto cadastrado.</p>';
        return;
    }

    container.innerHTML = allProducts.map(p => {
        const catName = allCategories.find(c => c.categoriaId === p.categoriaId)?.nome || 'Sem Categoria';
        return `
        <div class="product-card">
            <img class="product-img" src="${obterImagemProduto(p)}" alt="${p.nome}">
            <div class="product-info">
                <h4>${p.nome}</h4>
                <p>${p.descricao || 'Sem descrição'}</p>
            </div>
            <div class="product-meta">
                <span class="badge" style="background-color: var(--glass); color: var(--text-muted); border: 1px solid var(--border-color)">${catName}</span>
                <span class="product-price">R$ ${p.preco.toFixed(2).replace('.', ',')}</span>
                <div class="product-actions" style="margin-top: 0.5rem">
                    <button class="btn btn-outline" style="padding: 0.4rem 0.6rem" title="Editar"><i data-lucide="edit-2" style="width: 14px; height: 14px;"></i></button>
                    <button class="btn btn-outline" style="padding: 0.4rem 0.6rem; color: var(--status-cancel); border-color: var(--status-cancel-bg);" title="Remover"><i data-lucide="trash-2" style="width: 14px; height: 14px;"></i></button>
                </div>
            </div>
        </div>
        `;
    }).join('');

    lucide.createIcons();
}

// -- Categories Rendering --
function renderCategories() {
    const container = document.getElementById('categoriesListContainer');

    if (allCategories.length === 0) {
        container.innerHTML = '<p class="subtitle" style="grid-column: 1/-1; text-align: center; padding: 2rem;">Nenhuma categoria cadastrada.</p>';
        return;
    }

    container.innerHTML = allCategories.map(cat => {
        // Find products for this category
        const catProducts = allProducts.filter(p => p.categoriaId === cat.categoriaId);
        // Generate random color for display purposes based on ID
        const colors = ['#f59e0b', '#8b5cf6', '#3b82f6', '#10b981', '#ef4444', '#ec4899'];
        const catColor = colors[cat.categoriaId % colors.length];

        return `
        <div class="category-card">
            <div class="cat-header">
                <div class="cat-title">
                    <div class="color-indicator" style="background-color: ${catColor}"></div>
                    ${cat.nome}
                </div>
                <div>
                    <button class="icon-btn"><i data-lucide="edit-2" style="width: 16px; height: 16px;"></i></button>
                    <button class="icon-btn"><i data-lucide="trash-2" style="width: 16px; height: 16px; color: var(--status-cancel);"></i></button>
                </div>
            </div>
            
            <div class="cat-tags">
                ${catProducts.map(p => `
                    <div class="cat-tag" style="border-left: 3px solid ${catColor};">
                        ${p.nome}
                        <button><i data-lucide="x" style="width: 12px; height: 12px;"></i></button>
                    </div>
                `).join('')}
                ${catProducts.length === 0 ? '<span style="color:var(--text-muted); font-size: 0.8rem">Nenhum produto vinculado</span>' : ''}
            </div>
            
            <div class="cat-add-item">
                <select>
                    <option value="">Vincular produto...</option>
                    ${allProducts.filter(p => p.categoriaId !== cat.categoriaId).map(p => `
                        <option value="${p.produtoId}">${p.nome}</option>
                    `).join('')}
                </select>
                <button class="btn btn-dark" style="padding: 0.5rem 1rem">Vincular</button>
            </div>
        </div>
        `;
    }).join('');

    lucide.createIcons();
}

// -- Utility --
function getBadgeHtml(statusKey, rawStatus) {
    const conf = statusConfig[statusKey] || { bg: 'var(--glass)', color: 'var(--text-main)' };
    return `<span class="badge" style="background-color: ${conf.bg}; color: ${conf.color}; border: 1px solid ${conf.color}40">${rawStatus}</span>`;
}

// -- Order Details Modal --
window.abrirOrderModal = function (id) {
    const order = allOrders.find(o => o.pedidoId === id);
    if (!order) return;

    document.getElementById('orderModalTitle').textContent = `Detalhes do Pedido #${order.pedidoId}`;
    document.getElementById('orderModalClient').textContent = `Cliente: ${order.nomeCliente}`;

    let html = '';
    if (order.itens && order.itens.length > 0) {
        html = order.itens.map(i => `
            <div class="order-detail-row">
                <span>${i.quantidade}x ${i.nomeProduto}</span>
                <span>R$ ${i.subTotal.toFixed(2).replace('.', ',')}</span>
            </div>
        `).join('');
    } else {
        html = '<p style="color:var(--text-muted); font-size:0.9rem">Nenhum item encontrado.</p>';
    }

    document.getElementById('orderModalItems').innerHTML = html;
    document.getElementById('orderModalTotal').textContent = `Total: R$ ${order.valorTotal.toFixed(2).replace('.', ',')}`;

    document.getElementById('orderModal').style.display = 'flex';
}

window.fecharOrderModal = function () {
    document.getElementById('orderModal').style.display = 'none';
}

// -- Product Modal --
window.abrirProductModal = function () {
    // Populate categories select
    const select = document.getElementById('prodCat');
    select.innerHTML = '<option value="">Selecione uma categoria...</option>' +
        allCategories.map(c => `<option value="${c.categoriaId}">${c.nome}</option>`).join('');

    // Clear form
    document.getElementById('prodNome').value = '';
    document.getElementById('prodDesc').value = '';
    document.getElementById('prodPreco').value = '';
    document.getElementById('prodEstoque').value = '0';
    document.getElementById('prodImg').value = '';

    document.getElementById('productModal').style.display = 'flex';
}

window.fecharProductModal = function () {
    document.getElementById('productModal').style.display = 'none';
}

window.salvarNovoProduto = async function () {
    const nome = document.getElementById('prodNome').value.trim();
    const desc = document.getElementById('prodDesc').value.trim();
    const preco = parseFloat(document.getElementById('prodPreco').value);
    const estoque = parseInt(document.getElementById('prodEstoque').value);
    const catId = parseInt(document.getElementById('prodCat').value);
    let imgUrl = document.getElementById('prodImg').value.trim();

    if (!nome || nome.length < 5 || nome.length > 20) {
        alert("O nome deve ter entre 5 e 20 caracteres.");
        return;
    }
    if (isNaN(preco) || preco < 1) {
        alert("O preço deve ser pelo menos R$ 1,00.");
        return;
    }
    if (isNaN(catId)) {
        alert("Por favor, selecione uma categoria.");
        return;
    }
    if (!desc) {
        alert("A descrição é obrigatória.");
        return;
    }
    if (!imgUrl || imgUrl.length < 10) {
        imgUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=500&h=350&fit=crop"; // fallback válido
    }

    const payload = {
        nome: nome,
        descricao: desc,
        preco: preco,
        estoque: isNaN(estoque) ? 0 : estoque,
        categoriaId: catId,
        imageUrl: imgUrl
    };

    try {
        const response = await fetch(`${API_BASE}/api/Produtos`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${authToken}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            fecharProductModal();
            await carregarDadosIniciais();
            alert("Produto adicionado com sucesso!");
        } else {
            const err = await response.json();
            alert("Erro ao salvar produto: " + (err.message || ""));
        }
    } catch (e) {
        alert("Erro de conexão ao salvar.");
    }
}
