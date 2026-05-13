/** Valida o JWT com GET /api/Produtos antes de aplicar no Swagger UI. */
(function () {
    'use strict';

    function extrairToken(payload) {
        if (!payload || typeof payload !== 'object') {
            return null;
        }
        var bearerKey = Object.keys(payload).find(function (k) {
            return k.toLowerCase() === 'bearer';
        });
        if (!bearerKey) {
            return null;
        }
        var b = payload[bearerKey];
        if (!b) {
            return null;
        }
        var v = typeof b.value === 'string' ? b.value : (typeof b === 'string' ? b : '');
        v = (v || '').trim();
        if (!v) {
            return null;
        }
        if (v.toLowerCase().indexOf('bearer ') === 0) {
            v = v.substring(7).trim();
        }
        return v || null;
    }

    function aguardarUi(callback) {
        var n = 0;
        var id = setInterval(function () {
            n++;
            var ui = window.ui;
            if (ui && ui.authActions && typeof ui.authActions.authorize === 'function') {
                clearInterval(id);
                callback(ui);
            } else if (n > 400) {
                clearInterval(id);
            }
        }, 25);
    }

    window.addEventListener('load', function () {
        aguardarUi(function (ui) {
            var original = ui.authActions.authorize.bind(ui.authActions);
            ui.authActions.authorize = function (payload) {
                var token = extrairToken(payload);
                if (!token) {
                    return original(payload);
                }
                var url = window.location.origin + '/api/Produtos';
                fetch(url, { method: 'GET', headers: { Authorization: 'Bearer ' + token } })
                    .then(function (res) {
                        if (res.status === 200) {
                            return original(payload);
                        }
                        return res.json().then(function (body) {
                            var msg = (body && body.message) ? body.message : 'Token JWT inválido ou expirado.';
                            window.alert(msg);
                        }).catch(function () {
                            window.alert('Token JWT inválido ou expirado.');
                        });
                    })
                    .catch(function () {
                        window.alert('Não foi possível validar o token com a API (rede ou servidor).');
                    });
            };
        });
    });
})();
