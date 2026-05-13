// ============================================
// ESTADO GLOBAL
// ============================================
const API_BASE = window.location.origin;

function obterJwtDoArmazenamento() {
    const bruto = localStorage.getItem('jwt');
    return bruto && bruto.trim() ? bruto.trim() : '';
}

let authToken = obterJwtDoArmazenamento();
let carrinho = [];
let produtos = [];
let userRole = localStorage.getItem('jwt_role') || '';
let categoriaFiltro = null;
let categorias = [];
const qrCodeCache = new Map();

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
window.addEventListener('DOMContentLoaded', async () => {
    if (authToken) {
        const valido = await validarTokenArmazenado();
        if (valido) mostrarApp();
    }
});

async function lerMensagemErroApi(response) {
    try {
        const data = await response.clone().json();
        if (data && typeof data.message === 'string') return data.message;
    } catch { }
    return null;
}

function encerrarSessaoPorTokenInvalido(mensagemPadrao) {
    authToken = '';
    localStorage.removeItem('jwt');
    localStorage.removeItem('jwt_user');
    const appEl = document.getElementById('app');
    const overlayEl = document.getElementById('loginOverlay');
    if (appEl) appEl.style.display = 'none';
    if (overlayEl) overlayEl.style.display = 'flex';
    const errorEl = document.getElementById('loginError');
    if (errorEl) { errorEl.textContent = mensagemPadrao; errorEl.style.display = 'block'; }
}

async function validarTokenArmazenado() {
    authToken = obterJwtDoArmazenamento();
    if (!authToken) return false;
    try {
        const response = await fetch(`${API_BASE}/api/Produtos`, {
            headers: { Authorization: `Bearer ${authToken}` }
        });
        if (response.status === 401) {
            const msg = (await lerMensagemErroApi(response)) || 'Token inválido ou expirado. Faça login novamente.';
            encerrarSessaoPorTokenInvalido(msg);
            return false;
        }
        if (!response.ok) { encerrarSessaoPorTokenInvalido('Não foi possível validar a sessão.'); return false; }
        try { await response.json(); } catch { }
        return true;
    } catch { encerrarSessaoPorTokenInvalido('Erro de conexão ao validar o token.'); return false; }
}

// ============================================
// AUTH TABS (Login / Cadastro)
// ============================================
function mostrarTabLogin() {
    document.getElementById('tabLogin').classList.add('active');
    document.getElementById('tabCadastro').classList.remove('active');
    document.getElementById('formLogin').style.display = 'block';
    document.getElementById('formCadastro').style.display = 'none';
    document.getElementById('loginError').style.display = 'none';
    document.getElementById('cadastroSuccess').style.display = 'none';
}

function mostrarTabCadastro() {
    document.getElementById('tabCadastro').classList.add('active');
    document.getElementById('tabLogin').classList.remove('active');
    document.getElementById('formCadastro').style.display = 'block';
    document.getElementById('formLogin').style.display = 'none';
    document.getElementById('loginError').style.display = 'none';
    document.getElementById('cadastroSuccess').style.display = 'none';
}

function setLoginMode(mode) {
    document.getElementById('modeAdmin').classList.toggle('active', mode === 'admin');
    document.getElementById('modeCliente').classList.toggle('active', mode === 'cliente');
    document.getElementById('loginAdmin').style.display = mode === 'admin' ? 'block' : 'none';
    document.getElementById('loginCliente').style.display = mode === 'cliente' ? 'block' : 'none';
    document.getElementById('loginError').style.display = 'none';
}

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
        authToken = (data.token || '').trim();
        if (!authToken) { errorEl.textContent = 'Resposta sem token.'; errorEl.style.display = 'block'; return; }
        localStorage.setItem('jwt', authToken);
        localStorage.setItem('jwt_user', data.usuario);
        userRole = 'admin';
        localStorage.setItem('jwt_role', 'admin');
        mostrarApp();
    } catch (e) { errorEl.textContent = 'Erro de conexão com o servidor'; errorEl.style.display = 'block'; }
}

async function fazerLoginEmail() {
    const email = document.getElementById('loginEmail').value;
    const senha = document.getElementById('loginEmailPass').value;
    const errorEl = document.getElementById('loginError');
    errorEl.style.display = 'none';
    if (!email || !senha) { errorEl.textContent = 'Preencha email e senha.'; errorEl.style.display = 'block'; return; }
    try {
        const response = await fetch(`${API_BASE}/api/Auth/login-email`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, senha })
        });
        if (!response.ok) {
            const data = await response.json();
            errorEl.textContent = data.message || 'Email ou senha inválidos';
            errorEl.style.display = 'block';
            return;
        }
        const data = await response.json();
        authToken = (data.token || '').trim();
        if (!authToken) { errorEl.textContent = 'Resposta sem token.'; errorEl.style.display = 'block'; return; }
        localStorage.setItem('jwt', authToken);
        localStorage.setItem('jwt_user', data.usuario);
        userRole = 'cliente';
        localStorage.setItem('jwt_role', 'cliente');
        mostrarApp();
    } catch (e) { errorEl.textContent = 'Erro de conexão com o servidor'; errorEl.style.display = 'block'; }
}

async function fazerCadastro() {
    const nome = document.getElementById('cadastroNome').value.trim();
    const email = document.getElementById('cadastroEmail').value.trim();
    const senha = document.getElementById('cadastroSenha').value;
    const confirm = document.getElementById('cadastroConfirm').value;
    const errorEl = document.getElementById('loginError');
    const successEl = document.getElementById('cadastroSuccess');
    errorEl.style.display = 'none';
    successEl.style.display = 'none';

    if (!nome || !email || !senha) { errorEl.textContent = 'Preencha todos os campos.'; errorEl.style.display = 'block'; return; }
    if (senha.length < 6) { errorEl.textContent = 'A senha deve ter no mínimo 6 caracteres.'; errorEl.style.display = 'block'; return; }
    if (senha !== confirm) { errorEl.textContent = 'As senhas não coincidem.'; errorEl.style.display = 'block'; return; }

    try {
        const response = await fetch(`${API_BASE}/api/Auth/cadastro`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nome, email, senha })
        });
        if (!response.ok) {
            const data = await response.json();
            errorEl.textContent = data.message || 'Erro ao cadastrar';
            errorEl.style.display = 'block';
            return;
        }
        const data = await response.json();
        authToken = (data.token || '').trim();
        if (authToken) {
            localStorage.setItem('jwt', authToken);
            localStorage.setItem('jwt_user', data.nome);
            userRole = 'cliente';
            localStorage.setItem('jwt_role', 'cliente');
            successEl.textContent = `Cadastro realizado! Bem-vindo, ${data.nome}!`;
            successEl.style.display = 'block';
            setTimeout(() => mostrarApp(), 1000);
        } else {
            successEl.textContent = 'Cadastro realizado! Faça login para continuar.';
            successEl.style.display = 'block';
            setTimeout(() => mostrarTabLogin(), 1500);
        }
    } catch (e) { errorEl.textContent = 'Erro de conexão com o servidor'; errorEl.style.display = 'block'; }
}

function logout() {
    authToken = '';
    userRole = '';
    localStorage.removeItem('jwt');
    localStorage.removeItem('jwt_user');
    localStorage.removeItem('jwt_role');
    document.getElementById('app').style.display = 'none';
    document.getElementById('loginOverlay').style.display = 'flex';
}

function mostrarApp() {
    authToken = obterJwtDoArmazenamento();
    if (!authToken) {
        document.getElementById('loginOverlay').style.display = 'flex';
        document.getElementById('app').style.display = 'none';
        return;
    }
    document.getElementById('loginOverlay').style.display = 'none';
    document.getElementById('app').style.display = 'block';
    const nome = (localStorage.getItem('jwt_user') || '').trim();
    userRole = localStorage.getItem('jwt_role') || '';
    const badgeEl = document.getElementById('userBadge');
    badgeEl.textContent = nome || '—';
    if (userRole === 'admin') badgeEl.classList.add('admin-badge');
    else badgeEl.classList.remove('admin-badge');
    const ordersTitleEl = document.getElementById('ordersTitle');
    if (ordersTitleEl) ordersTitleEl.textContent = userRole === 'admin' ? 'Todos os Pedidos (Admin)' : 'Meus Pedidos';
    // Admin navbar
    const adminNav = document.getElementById('adminNavbar');
    if (adminNav) adminNav.style.display = userRole === 'admin' ? 'flex' : 'none';
    // Reset to inicio tab
    navAdmin('inicio');
    carregarProdutos();
    carregarCategorias();
    carregarPedidos();
    if (userRole === 'admin') carregarEstoque();
}

function navAdmin(tab, btn) {
    // Esconde todas as tabs
    ['tabInicio', 'tabPedidos', 'tabEstoque'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.style.display = 'none';
    });
    // Mostra a tab selecionada
    const targetId = tab === 'inicio' ? 'tabInicio' : tab === 'pedidos' ? 'tabPedidos' : 'tabEstoque';
    const target = document.getElementById(targetId);
    if (target) target.style.display = 'block';
    // Hero banner só aparece na tab inicio
    const hero = document.getElementById('heroBanner');
    if (hero) hero.style.display = tab === 'inicio' ? 'block' : 'none';
    // Atualiza botões ativos
    document.querySelectorAll('.admin-nav-btn').forEach(b => b.classList.remove('active'));
    if (btn) btn.classList.add('active');
    else {
        const autoBtn = document.querySelector(`.admin-nav-btn[data-tab="${tab}"]`);
        if (autoBtn) autoBtn.classList.add('active');
    }
    // Recarrega dados da tab
    if (tab === 'pedidos') carregarPedidos();
    if (tab === 'estoque') carregarEstoque();
}

// ============================================
// PRODUTOS
// ============================================
async function carregarProdutos() {
    try {
        const response = await fetch(`${API_BASE}/api/Produtos`, { headers: { 'Authorization': `Bearer ${authToken}` } });
        if (response.status === 401) {
            const msg = (await lerMensagemErroApi(response)) || 'Token inválido ou expirado.';
            mostrarToast(msg, 'error'); encerrarSessaoPorTokenInvalido(msg); return;
        }
        if (!response.ok) throw new Error('Erro ao carregar');
        produtos = await response.json();
        renderizarProdutos();
    } catch (e) {
        document.getElementById('productsGrid').innerHTML = '<p style="color:var(--text3)">Erro ao carregar produtos.</p>';
    }
}

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

function renderizarProdutos() {
    const grid = document.getElementById('productsGrid');
    if (!produtos.length) { grid.innerHTML = '<p style="color:var(--text3)">Nenhum produto cadastrado.</p>'; return; }
    grid.innerHTML = produtos.map(p => `
        <div class="product-card">
            <div class="product-image-wrap">
                <img class="product-image" src="${obterImagemProduto(p)}" alt="${escapeHtml(p.nome)}" loading="lazy" onerror="this.src='${FOOD_IMAGES['default']}'">
            </div>
            <div class="product-body">
                <div class="product-name">${escapeHtml(p.nome)}</div>
                <div class="product-desc">${escapeHtml(p.descricao || 'Delicioso!')}</div>
                <div class="product-footer">
                    <span class="product-price">R$ ${formatarPreco(p.preco)}</span>
                    <span class="product-stock">${Math.floor(p.estoque)} un.</span>
                </div>
                <button class="btn btn-primary product-add-btn" onclick="adicionarAoCarrinho(${p.produtoId})" ${p.estoque < 1 ? 'disabled' : ''}>
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
        if (existente.quantidade >= produto.estoque) { mostrarToast('Estoque insuficiente!', 'error'); return; }
        existente.quantidade++;
    } else {
        carrinho.push({ id: produtoId, nome: produto.nome, preco: produto.preco, quantidade: 1, estoqueMax: Math.floor(produto.estoque), imagem: obterImagemProduto(produto) });
    }
    renderizarCarrinho();
    mostrarToast(`${produto.nome} adicionado!`, 'success');
}

function alterarQuantidade(produtoId, delta) {
    const item = carrinho.find(c => c.id === produtoId);
    if (!item) return;
    item.quantidade += delta;
    if (item.quantidade <= 0) carrinho = carrinho.filter(c => c.id !== produtoId);
    renderizarCarrinho();
}

function renderizarCarrinho() {
    const body = document.getElementById('cartBody');
    const countEl = document.getElementById('cartCount');
    const totalEl = document.getElementById('cartTotal');
    const btnFinalizar = document.getElementById('btnFinalizar');
    if (carrinho.length === 0) {
        body.innerHTML = `<div class="cart-empty"><div class="cart-empty-icon"><svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" opacity="0.3"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg></div>Seu carrinho está vazio<br><small>Adicione itens do cardápio</small></div>`;
        countEl.style.display = 'none'; totalEl.textContent = 'R$ 0,00'; btnFinalizar.disabled = true; return;
    }
    const totalItens = carrinho.reduce((sum, c) => sum + c.quantidade, 0);
    countEl.textContent = totalItens; countEl.style.display = 'inline-flex'; btnFinalizar.disabled = false;
    let valorTotal = 0;
    body.innerHTML = carrinho.map(item => {
        const subtotal = item.preco * item.quantidade; valorTotal += subtotal;
        return `<div class="cart-item"><img class="cart-item-img" src="${item.imagem}" alt="${escapeHtml(item.nome)}"><div class="cart-item-info"><div class="cart-item-name">${escapeHtml(item.nome)}</div><div class="cart-item-price">R$ ${formatarPreco(item.preco)} × ${item.quantidade} = <b>R$ ${formatarPreco(subtotal)}</b></div></div><div class="cart-qty-controls"><button class="qty-btn" onclick="alterarQuantidade(${item.id}, -1)">−</button><span class="qty-value">${item.quantidade}</span><button class="qty-btn" onclick="alterarQuantidade(${item.id}, 1)" ${item.quantidade >= item.estoqueMax ? 'disabled' : ''}>+</button></div></div>`;
    }).join('');
    totalEl.textContent = `R$ ${formatarPreco(valorTotal)}`;
}

// ============================================
// PEDIDOS
// ============================================
async function finalizarPedido() {
    const nomeCliente = document.getElementById('nomeCliente').value.trim();
    if (!nomeCliente) { mostrarToast('Informe o nome do cliente!', 'error'); document.getElementById('nomeCliente').focus(); return; }
    if (carrinho.length === 0) return;
    const btn = document.getElementById('btnFinalizar');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner"></span> Processando...';
    const payload = { nomeCliente, itens: carrinho.map(c => ({ produtoId: c.id, quantidade: c.quantidade })) };
    try {
        const response = await fetch(`${API_BASE}/api/Pedido`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${authToken}` },
            body: JSON.stringify(payload)
        });
        if (response.status === 401) {
            const msg = (await lerMensagemErroApi(response)) || 'Sessão expirada.';
            mostrarToast(msg, 'error'); encerrarSessaoPorTokenInvalido(msg); return;
        }
        if (!response.ok) { const err = await response.json(); mostrarToast(err.message || 'Erro ao criar pedido', 'error'); return; }
        const pedido = await response.json();
        mostrarToast(`Pedido #${pedido.pedidoId} criado! Total: R$ ${formatarPreco(pedido.valorTotal)}`, 'success');
        // Exibe QR Code
        if (pedido.qrCodeBase64) abrirQrModal(pedido.pedidoId, pedido.valorTotal, pedido.qrCodeBase64);
        carrinho = [];
        document.getElementById('nomeCliente').value = '';
        renderizarCarrinho();
        carregarProdutos();
        carregarPedidos();
    } catch (e) { mostrarToast('Erro de conexão com o servidor', 'error'); }
    finally {
        btn.disabled = false;
        btn.innerHTML = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg> Finalizar Pedido';
    }
}

let pedidoParaCancelar = null;

function cancelarPedido(id) {
    pedidoParaCancelar = id;
    document.getElementById('cancelModalPedidoId').textContent = `#${id}`;
    document.getElementById('cancelModal').style.display = 'flex';
}

function fecharCancelModal() {
    document.getElementById('cancelModal').style.display = 'none';
    pedidoParaCancelar = null;
}

async function confirmarCancelamento() {
    const id = pedidoParaCancelar;
    if (!id) return;
    fecharCancelModal();
    try {
        const response = await fetch(`${API_BASE}/api/Pedido/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        if (response.status === 401) {
            const msg = (await lerMensagemErroApi(response)) || 'Sessão expirada.';
            mostrarToast(msg, 'error'); encerrarSessaoPorTokenInvalido(msg); return;
        }
        if (!response.ok) { const err = await response.json(); mostrarToast(err.message || 'Erro ao cancelar', 'error'); return; }
        const data = await response.json();
        mostrarToast(data.message || `Pedido #${id} cancelado!`, 'success');
        carregarProdutos();
        carregarPedidos();
        if (userRole === 'admin') carregarEstoque();
    } catch (e) { mostrarToast('Erro de conexão', 'error'); }
}

async function carregarPedidos() {
    try {
        let url = `${API_BASE}/api/Pedido`;
        if (categoriaFiltro) url += `?categoriaId=${categoriaFiltro}`;
        const response = await fetch(url, { headers: { 'Authorization': `Bearer ${authToken}` } });
        if (response.status === 401) {
            const msg = (await lerMensagemErroApi(response)) || 'Token inválido ou expirado.';
            mostrarToast(msg, 'error'); encerrarSessaoPorTokenInvalido(msg); return;
        }
        if (!response.ok) return;
        const pedidos = await response.json();
        renderizarPedidos(pedidos);
    } catch (e) { }
}

function renderizarPedidos(pedidos) {
    // Renderiza em todos os containers de pedidos visíveis
    const containers = ['ordersList', 'ordersListPedidos'];
    const emptyHtml = `<div class="order-empty"><div class="order-empty-icon"><svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" opacity="0.3"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg></div>Nenhum pedido realizado ainda</div>`;

    if (!pedidos || !pedidos.length) {
        containers.forEach(id => { const el = document.getElementById(id); if (el) el.innerHTML = emptyHtml; });
        return;
    }
    pedidos.forEach(p => { if (p.qrCodeBase64) qrCodeCache.set(p.pedidoId, p.qrCodeBase64); });
    const isCancelado = (s) => s && s.startsWith('Cancelado');
    const statusClass = (s) => isCancelado(s) ? 'status-cancelled' : s === 'Pendente' ? 'status-pending' : '';
    const html = pedidos.map(p => `
        <div class="order-card ${isCancelado(p.status) ? 'order-cancelled' : ''}">
            <div class="order-header">
                <span>
                    <span class="order-id">Pedido #${p.pedidoId}</span>
                    <span class="order-client"> — ${escapeHtml(p.nomeCliente)}</span>
                </span>
                <span class="order-status ${statusClass(p.status)}">● ${escapeHtml(p.status)}</span>
            </div>
            <div class="order-items">
                ${p.itens.map(i => `<span class="order-item-tag">${i.quantidade}× ${escapeHtml(i.nomeProduto)} <b>R$ ${formatarPreco(i.subTotal)}</b></span>`).join('')}
            </div>
            <div class="order-total">
                <span class="order-total-value">Total: R$ ${formatarPreco(p.valorTotal)}</span>
                <span class="order-date">${new Date(p.dataPedido).toLocaleString('pt-BR')}</span>
            </div>
            <div class="order-actions">
                ${!isCancelado(p.status) && p.qrCodeBase64 ? `<button class="btn btn-secondary btn-sm" onclick="abrirQrModalCache(${p.pedidoId}, ${p.valorTotal})"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg> QR Code</button>` : ''}
                ${!isCancelado(p.status) ? `<button class="btn btn-danger-outline btn-sm" onclick="cancelarPedido(${p.pedidoId})">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                    Cancelar
                </button>` : ''}
            </div>
        </div>
    `).join('');
    containers.forEach(id => { const el = document.getElementById(id); if (el) el.innerHTML = html; });
}

// ============================================
// QR CODE MODAL
// ============================================
function abrirQrModal(pedidoId, valor, base64) {
    document.getElementById('qrModalImg').src = `data:image/png;base64,${base64}`;
    document.getElementById('qrPedidoId').textContent = `#${pedidoId}`;
    document.getElementById('qrValor').textContent = `R$ ${formatarPreco(valor)}`;
    document.getElementById('qrModal').style.display = 'flex';
}

function abrirQrModalCache(pedidoId, valor) {
    const base64 = qrCodeCache.get(pedidoId);
    if (base64) abrirQrModal(pedidoId, valor, base64);
    else mostrarToast('QR Code não disponível.', 'error');
}

function fecharQrModal() {
    document.getElementById('qrModal').style.display = 'none';
}

// ============================================
// CATEGORIAS + FILTRO
// ============================================
async function carregarCategorias() {
    try {
        const response = await fetch(`${API_BASE}/api/Categorias`, { headers: { 'Authorization': `Bearer ${authToken}` } });
        if (!response.ok) return;
        categorias = await response.json();
        renderizarFiltrosCategorias();
    } catch (e) { }
}

function renderizarFiltrosCategorias() {
    const btnHtml = `<button class="filter-btn ${!categoriaFiltro ? 'active' : ''}" onclick="filtrarPorCategoria(null, this)">Todos</button>`
        + categorias.map(c => `<button class="filter-btn ${categoriaFiltro === c.categoriaId ? 'active' : ''}" onclick="filtrarPorCategoria(${c.categoriaId}, this)">${escapeHtml(c.nome)}</button>`).join('');
    ['categoryFilter', 'categoryFilterPedidos'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = btnHtml;
    });
}

function filtrarPorCategoria(catId, btn) {
    categoriaFiltro = catId;
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    if (btn) btn.classList.add('active');
    carregarPedidos();
}

// ============================================
// GESTÃO DE ESTOQUE (Admin)
// ============================================
async function carregarEstoque() {
    try {
        const response = await fetch(`${API_BASE}/api/Produtos`, { headers: { 'Authorization': `Bearer ${authToken}` } });
        if (!response.ok) return;
        const prods = await response.json();
        renderizarEstoque(prods);
    } catch (e) { }
}

function renderizarEstoque(prods) {
    const grid = document.getElementById('stockGrid');
    if (!grid) return;
    grid.innerHTML = prods.map(p => `
        <div class="stock-item">
            <img class="stock-item-img" src="${obterImagemProduto(p)}" alt="${escapeHtml(p.nome)}" onerror="this.src='${FOOD_IMAGES['default']}'">
            <div class="stock-item-info">
                <div class="stock-item-name">${escapeHtml(p.nome)}</div>
                <div class="stock-item-price">R$ ${formatarPreco(p.preco)}</div>
            </div>
            <div class="stock-item-controls">
                <label class="stock-label">Estoque:</label>
                <input type="number" class="stock-input" id="estoque-${p.produtoId}" value="${Math.floor(p.estoque)}" min="0">
                <button class="btn btn-primary btn-sm" onclick="atualizarEstoque(${p.produtoId})">Salvar</button>
            </div>
        </div>
    `).join('');
}

async function atualizarEstoque(produtoId) {
    const input = document.getElementById(`estoque-${produtoId}`);
    if (!input) return;
    const novoEstoque = parseInt(input.value);
    if (isNaN(novoEstoque) || novoEstoque < 0) { mostrarToast('Valor inválido', 'error'); return; }
    try {
        const response = await fetch(`${API_BASE}/api/Produtos/${produtoId}/estoque`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${authToken}` },
            body: JSON.stringify({ estoque: novoEstoque })
        });
        if (!response.ok) { const err = await response.json(); mostrarToast(err.message || 'Erro', 'error'); return; }
        const data = await response.json();
        mostrarToast(data.message || 'Estoque atualizado!', 'success');
        carregarProdutos();
    } catch (e) { mostrarToast('Erro de conexão', 'error'); }
}

// ============================================
// UTILITÁRIOS
// ============================================
function formatarPreco(valor) { return Number(valor).toFixed(2).replace('.', ','); }

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
