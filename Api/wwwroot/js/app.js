// ============================================
// ESTADO GLOBAL
// ============================================
const API_BASE = window.location.origin;
let authToken = localStorage.getItem('jwt') || '';
let carrinho = [];
let produtos = [];

// Mapa de imagens reais do Unsplash para produtos
const FOOD_IMAGES = {
    'smash':   'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'classico':'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'bbq':     'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=500&h=350&fit=crop',
    'bacon':   'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=500&h=350&fit=crop',
    'trufa':   'https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=500&h=350&fit=crop',
    'supreme': 'https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=500&h=350&fit=crop',
    'hambur':  'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'burger':  'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=350&fit=crop',
    'lanche':  'https://images.unsplash.com/photo-1550547660-d9450f859349?w=500&h=350&fit=crop',
    'batata':  'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'rustica': 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'frita':   'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=350&fit=crop',
    'onion':   'https://images.unsplash.com/photo-1541592106381-b31e9677c0e5?w=500&h=350&fit=crop',
    'refri':   'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&h=350&fit=crop',
    'coca':    'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500&h=350&fit=crop',
    'milkshake':'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?w=500&h=350&fit=crop',
    'sorvete': 'https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?w=500&h=350&fit=crop',
    'suco':    'https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=500&h=350&fit=crop',
    'default': 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=500&h=350&fit=crop'
};

// ============================================
// INICIALIZAÇÃO
// ============================================
window.addEventListener('DOMContentLoaded', () => {
    if (authToken) {
        mostrarApp();
    }
});

// ============================================
// AUTENTICAÇÃO
// ============================================
async function fazerLogin() {
    const usuario = document.getElementById('loginUser').value;
    const senha = document.getElementById('loginPass').value;
    const errorEl = document.getElementById('loginError');
    errorEl.style.display = 'none';

    try {
        const response = await fetch(`${API_BASE}/api/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ usuario, senha })
        });

        if (!response.ok) {
            const data = await response.json();
            errorEl.textContent = data.message || 'Credenciais inválidas';
            errorEl.style.display = 'block';
            return;
        }

        const data = await response.json();
        authToken = data.token;
        localStorage.setItem('jwt', authToken);
        localStorage.setItem('jwt_user', data.usuario);
        mostrarApp();
    } catch (e) {
        errorEl.textContent = 'Erro de conexão com o servidor';
        errorEl.style.display = 'block';
    }
}

function logout() {
    authToken = '';
    localStorage.removeItem('jwt');
    localStorage.removeItem('jwt_user');
    document.getElementById('app').style.display = 'none';
    document.getElementById('loginOverlay').style.display = 'flex';
}

function mostrarApp() {
    document.getElementById('loginOverlay').style.display = 'none';
    document.getElementById('app').style.display = 'block';
    document.getElementById('userBadge').textContent =
        localStorage.getItem('jwt_user') || 'admin';
    carregarProdutos();
    carregarPedidos();
}

// ============================================
// PRODUTOS
// ============================================
async function carregarProdutos() {
    try {
        const response = await fetch(`${API_BASE}/Produtos`);
        if (!response.ok) throw new Error('Erro ao carregar');
        produtos = await response.json();
        renderizarProdutos();
    } catch (e) {
        document.getElementById('productsGrid').innerHTML =
            '<p style="color:var(--text3)">Erro ao carregar produtos. Verifique se a API está rodando.</p>';
    }
}

function obterImagemProduto(produto) {
    // Prioriza a imageUrl do banco de dados
    if (typeof produto === 'object' && produto.imageUrl && produto.imageUrl.startsWith('http')) {
        return produto.imageUrl;
    }
    const nome = typeof produto === 'string' ? produto : produto?.nome;
    if (!nome) return FOOD_IMAGES['default'];
    const nomeLower = nome.toLowerCase();
    for (const [chave, url] of Object.entries(FOOD_IMAGES)) {
        if (chave !== 'default' && nomeLower.includes(chave)) {
            return url;
        }
    }
    return FOOD_IMAGES['default'];
}

function renderizarProdutos() {
    const grid = document.getElementById('productsGrid');
    if (!produtos.length) {
        grid.innerHTML = '<p style="color:var(--text3)">Nenhum produto cadastrado.</p>';
        return;
    }

    grid.innerHTML = produtos.map(p => `
        <div class="product-card">
            <div class="product-image-wrap">
                <img class="product-image"
                     src="${obterImagemProduto(p)}"
                     alt="${escapeHtml(p.nome)}"
                     loading="lazy"
                     onerror="this.src='${FOOD_IMAGES['default']}'">
            </div>
            <div class="product-body">
                <div class="product-name">${escapeHtml(p.nome)}</div>
                <div class="product-desc">${escapeHtml(p.descricao || 'Delicioso!')}</div>
                <div class="product-footer">
                    <span class="product-price">R$ ${formatarPreco(p.preco)}</span>
                    <span class="product-stock">${Math.floor(p.estoque)} un.</span>
                </div>
                <button class="btn btn-primary product-add-btn"
                        onclick="adicionarAoCarrinho(${p.produtoId})"
                        ${p.estoque < 1 ? 'disabled' : ''}>
                    ${p.estoque < 1 ? 'Esgotado' : '+ Adicionar'}
                </button>
            </div>
        </div>
    `).join('');
}

// ============================================
// CARRINHO
// ============================================
function adicionarAoCarrinho(produtoId) {
    const produto = produtos.find(p => p.produtoId === produtoId);
    if (!produto) return;

    const existente = carrinho.find(c => c.id === produtoId);
    if (existente) {
        if (existente.quantidade >= produto.estoque) {
            mostrarToast('Estoque insuficiente!', 'error');
            return;
        }
        existente.quantidade++;
    } else {
        carrinho.push({
            id: produtoId,
            nome: produto.nome,
            preco: produto.preco,
            quantidade: 1,
            estoqueMax: Math.floor(produto.estoque),
            imagem: obterImagemProduto(produto)
        });
    }

    renderizarCarrinho();
    mostrarToast(`${produto.nome} adicionado!`, 'success');
}

function alterarQuantidade(produtoId, delta) {
    const item = carrinho.find(c => c.id === produtoId);
    if (!item) return;

    item.quantidade += delta;
    if (item.quantidade <= 0) {
        carrinho = carrinho.filter(c => c.id !== produtoId);
    }

    renderizarCarrinho();
}

function renderizarCarrinho() {
    const body = document.getElementById('cartBody');
    const countEl = document.getElementById('cartCount');
    const totalEl = document.getElementById('cartTotal');
    const btnFinalizar = document.getElementById('btnFinalizar');

    if (carrinho.length === 0) {
        body.innerHTML = `
            <div class="cart-empty">
                <div class="cart-empty-icon">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" opacity="0.3"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg>
                </div>
                Seu carrinho está vazio
                <br><small>Adicione itens do cardápio</small>
            </div>`;
        countEl.style.display = 'none';
        totalEl.textContent = 'R$ 0,00';
        btnFinalizar.disabled = true;
        return;
    }

    const totalItens = carrinho.reduce((sum, c) => sum + c.quantidade, 0);
    countEl.textContent = totalItens;
    countEl.style.display = 'inline-flex';
    btnFinalizar.disabled = false;

    let valorTotal = 0;
    body.innerHTML = carrinho.map(item => {
        const subtotal = item.preco * item.quantidade;
        valorTotal += subtotal;
        return `
            <div class="cart-item">
                <img class="cart-item-img" src="${item.imagem}" alt="${escapeHtml(item.nome)}">
                <div class="cart-item-info">
                    <div class="cart-item-name">${escapeHtml(item.nome)}</div>
                    <div class="cart-item-price">
                        R$ ${formatarPreco(item.preco)} × ${item.quantidade} = <b>R$ ${formatarPreco(subtotal)}</b>
                    </div>
                </div>
                <div class="cart-qty-controls">
                    <button class="qty-btn" onclick="alterarQuantidade(${item.id}, -1)">−</button>
                    <span class="qty-value">${item.quantidade}</span>
                    <button class="qty-btn" onclick="alterarQuantidade(${item.id}, 1)"
                            ${item.quantidade >= item.estoqueMax ? 'disabled' : ''}>+</button>
                </div>
            </div>`;
    }).join('');

    totalEl.textContent = `R$ ${formatarPreco(valorTotal)}`;
}

// ============================================
// PEDIDOS
// ============================================
async function finalizarPedido() {
    const nomeCliente = document.getElementById('nomeCliente').value.trim();
    if (!nomeCliente) {
        mostrarToast('Informe o nome do cliente!', 'error');
        document.getElementById('nomeCliente').focus();
        return;
    }
    if (carrinho.length === 0) return;

    const btn = document.getElementById('btnFinalizar');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner"></span> Processando...';

    const payload = {
        nomeCliente,
        itens: carrinho.map(c => ({ produtoId: c.id, quantidade: c.quantidade }))
    };

    try {
        const response = await fetch(`${API_BASE}/api/Pedido`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify(payload)
        });

        if (response.status === 401) {
            mostrarToast('Sessão expirada! Faça login novamente.', 'error');
            logout();
            return;
        }

        if (!response.ok) {
            const err = await response.json();
            mostrarToast(err.message || 'Erro ao criar pedido', 'error');
            return;
        }

        const pedido = await response.json();
        mostrarToast(`Pedido #${pedido.pedidoId} criado! Total: R$ ${formatarPreco(pedido.valorTotal)}`, 'success');

        carrinho = [];
        document.getElementById('nomeCliente').value = '';
        renderizarCarrinho();
        carregarProdutos();
        carregarPedidos();
    } catch (e) {
        mostrarToast('Erro de conexão com o servidor', 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg> Finalizar Pedido';
    }
}

async function carregarPedidos() {
    try {
        const response = await fetch(`${API_BASE}/api/Pedido`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        if (!response.ok) return;
        const pedidos = await response.json();
        renderizarPedidos(pedidos);
    } catch (e) { /* silencioso */ }
}

function renderizarPedidos(pedidos) {
    const lista = document.getElementById('ordersList');
    if (!pedidos || !pedidos.length) {
        lista.innerHTML = `
            <div class="order-empty">
                <div class="order-empty-icon">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" opacity="0.3"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
                </div>
                Nenhum pedido realizado ainda
            </div>`;
        return;
    }

    lista.innerHTML = pedidos.map(p => `
        <div class="order-card">
            <div class="order-header">
                <span>
                    <span class="order-id">Pedido #${p.pedidoId}</span>
                    <span class="order-client"> — ${escapeHtml(p.nomeCliente)}</span>
                </span>
                <span class="order-status">● ${escapeHtml(p.status)}</span>
            </div>
            <div class="order-items">
                ${p.itens.map(i => `
                    <span class="order-item-tag">
                        ${i.quantidade}× ${escapeHtml(i.nomeProduto)} <b>R$ ${formatarPreco(i.subTotal)}</b>
                    </span>
                `).join('')}
            </div>
            <div class="order-total">
                <span class="order-total-value">Total: R$ ${formatarPreco(p.valorTotal)}</span>
                <span class="order-date">${new Date(p.dataPedido).toLocaleString('pt-BR')}</span>
            </div>
        </div>
    `).join('');
}

// ============================================
// UTILITÁRIOS
// ============================================
function formatarPreco(valor) {
    return Number(valor).toFixed(2).replace('.', ',');
}

function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function mostrarToast(mensagem, tipo) {
    const toast = document.getElementById('toast');
    toast.textContent = mensagem;
    toast.className = `toast toast-${tipo} show`;
    setTimeout(() => toast.classList.remove('show'), 3500);
}
