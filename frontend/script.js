// Конфигурация
if (!window.APP_CONFIG || !window.APP_CONFIG.apiBaseUrl) {
    console.error('API base URL не настроен. Проверьте config.js');
    // Показываем сообщение в интерфейсе, если функция уже определена (но в начале файла она ещё не определена)
    alert('Ошибка конфигурации: не удалось загрузить адрес API. Проверьте файл config.js');
    throw new Error('Конфигурация приложения не загружена');
}
const API_BASE_URL = window.APP_CONFIG.apiBaseUrl;

// ID пользователя (можно оставить как есть или тоже вынести в конфиг)
const USER_ID = 'user123';

// Состояние приложения
let currentBtcRate = null;

// Хранилище ключей идемпотентности для купленных скинов
const purchasedSkins = new Set();

// Вспомогательные функции
function getHeaders() {
    return {
        'Content-Type': 'application/json',
        'X-User-Id': USER_ID
    };
}

// Генерация уникального ключа идемпотентности
function generateIdempotencyKey(skinId) {
    const timestamp = Date.now();
    const random = Math.random().toString(36).substring(2, 10);
    return `${USER_ID}-${skinId}-${timestamp}-${random}`;
}

// Показать уведомление
function showNotification(message, type = 'success') {
    const notification = document.getElementById('notification');
    const icon = document.getElementById('notification-icon');
    const messageEl = document.getElementById('notification-message');

    icon.className = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle';
    notification.className = `notification ${type}`;
    messageEl.textContent = message;

    notification.classList.add('show');

    setTimeout(() => {
        notification.classList.remove('show');
    }, 3000);
}

// Показать детали покупки (по ТЗ)
function showPurchaseDetails(purchase) {
    // Создаем модальное окно
    const modal = document.createElement('div');
    modal.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0,0,0,0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
    `;

    const date = new Date(purchase.purchasedAt);
    modal.innerHTML = `
        <div style="
            background: white;
            padding: 2rem;
            border-radius: 20px;
            max-width: 400px;
            width: 90%;
            box-shadow: 0 20px 40px rgba(0,0,0,0.3);
        ">
            <h3 style="color: #667eea; margin-bottom: 1.5rem;">✅ Покупка совершена!</h3>
            <p><strong>ID:</strong> ${purchase.id}</p>
            <p><strong>Цена:</strong> $${purchase.finalPrice}</p>
            <p><strong>Время:</strong> ${date.toLocaleString()}</p>
            <p><strong>Курс BTC:</strong> $${purchase.btcRate}</p>
            <button onclick="this.closest('div').closest('div').remove()" style="
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: white;
                border: none;
                padding: 0.75rem 1.5rem;
                border-radius: 10px;
                font-weight: 600;
                cursor: pointer;
                margin-top: 1rem;
                width: 100%;
            ">Закрыть</button>
        </div>
    `;

    document.body.appendChild(modal);

    // Закрытие по клику вне модалки
    modal.addEventListener('click', (e) => {
        if (e.target === modal) modal.remove();
    });
}

// Переключение вкладок
function switchTab(tabName) {
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    document.getElementById(`${tabName}-tab`).classList.add('active');

    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    document.getElementById(`${tabName}-content`).classList.add('active');

    if (tabName === 'purchases') {
        loadPurchases();
    }
}

// Загрузка курса BTC
async function loadBtcRate() {
    const btcInfo = document.querySelector('#btc-rate-info span');

    try {
        const response = await fetch(`${API_BASE_URL}/rates/btc-usd`);
        if (!response.ok) throw new Error('Не удалось загрузить курс');

        const data = await response.json();
        currentBtcRate = data;

        let sourceIcon = '';
        switch(data.source) {
            case 'Cache': sourceIcon = '💾'; break;
            case 'External': sourceIcon = '🌐'; break;
            case 'Fallback': sourceIcon = '⚠️'; break;
            default: sourceIcon = '';
        }

        btcInfo.innerHTML = `1 BTC = $${data.rate.toLocaleString()} ${sourceIcon}`;

        if (data.ageSeconds) {
            btcInfo.innerHTML += ` (${data.ageSeconds} сек)`;
        }
    } catch (error) {
        console.error('Ошибка загрузки курса:', error);
        btcInfo.innerHTML = '❓ Курс временно недоступен';
    }
}

// Загрузка скинов
async function loadSkins() {
    const skinsGrid = document.getElementById('skins-grid');
    const errorDiv = document.getElementById('skins-error');

    skinsGrid.innerHTML = `
        <div class="loading">
            <i class="fas fa-spinner"></i>
            <p>Загрузка скинов...</p>
        </div>
    `;
    errorDiv.classList.add('d-none');

    try {
        const response = await fetch(`${API_BASE_URL}/skins?availableOnly=true`, {
            headers: getHeaders()
        });

        if (!response.ok) {
            throw new Error(`Ошибка ${response.status}`);
        }

        const skins = await response.json();

        if (skins.length === 0) {
            skinsGrid.innerHTML = `
                <div class="loading">
                    <i class="fas fa-box-open"></i>
                    <p>Нет доступных скинов</p>
                </div>
            `;
            return;
        }

        skinsGrid.innerHTML = '';

        skins.forEach((skin, index) => {
            const card = document.createElement('div');
            card.className = 'skin-card';
            card.style.animationDelay = `${index * 0.1}s`;

            // Проверяем, куплен ли уже этот скин
            const isPurchased = purchasedSkins.has(skin.id);

            // Выбираем иконку для скина
            let icon = 'fa-cube';
            if (skin.name.toLowerCase().includes('creeper')) icon = 'fa-face-smile';
            else if (skin.name.toLowerCase().includes('ender')) icon = 'fa-eye';
            else if (skin.name.toLowerCase().includes('dragon')) icon = 'fa-dragon';
            else if (skin.name.toLowerCase().includes('pig')) icon = 'fa-piggy-bank';

            card.innerHTML = `
                <div class="skin-image">
                    <i class="fas ${icon}"></i>
                </div>
                <div class="skin-info">
                    <h3>${skin.name}</h3>
                    <div class="skin-base-price">$${skin.basePriceUsd.toFixed(2)}</div>
                    <div class="skin-final-price">$${skin.finalPriceUsd.toFixed(2)}</div>
                    <button class="buy-btn" 
                            data-skin-id="${skin.id}"
                            ${isPurchased ? 'disabled' : ''}>
                        <i class="fas ${isPurchased ? 'fa-check' : 'fa-shopping-cart'}"></i>
                        ${isPurchased ? 'Уже куплено' : 'Купить сейчас'}
                    </button>
                </div>
            `;

            skinsGrid.appendChild(card);
        });

        // Добавляем обработчики на кнопки
        document.querySelectorAll('.buy-btn:not([disabled])').forEach(btn => {
            btn.addEventListener('click', buySkin);
        });

    } catch (error) {
        console.error('Ошибка загрузки скинов:', error);
        errorDiv.querySelector('span').textContent = ' Не удалось загрузить скины';
        errorDiv.classList.remove('d-none');
    }
}

async function buySkin(event) {
    const button = event.currentTarget;
    const skinId = button.dataset.skinId;
    const originalText = button.innerHTML;

    // Генерируем уникальный ключ для этой операции
    // Ключ генерируется ОДИН раз и НЕ УДАЛЯЕТСЯ после покупки
    const idempotencyKey = generateIdempotencyKey(skinId);

    console.log('🔑 Ключ идемпотентности:', idempotencyKey);

    button.disabled = true;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Покупка...';

    try {
        const response = await fetch(`${API_BASE_URL}/purchases`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-User-Id': USER_ID,
                'Idempotency-Key': idempotencyKey  // Один ключ на всю операцию
            },
            body: JSON.stringify({ skinId })
        });

        const responseData = await response.json();

        if (!response.ok) {
            if (response.status === 409) {
                throw new Error('Скин недоступен для покупки');
            } else if (response.status === 503) {
                throw new Error('Сервис курса валют временно недоступен');
            } else {
                throw new Error(responseData.detail || `Ошибка ${response.status}`);
            }
        }

        // Запоминаем, что скин куплен
        purchasedSkins.add(skinId);

        // Меняем кнопку на "Уже куплено"
        button.innerHTML = '<i class="fas fa-check"></i> Уже куплено';

        // Показываем уведомление
        showNotification(
            `✅ Скин куплен! Цена: $${responseData.finalPrice}`,
            'success'
        );

        // Показываем детали покупки (по ТЗ)
        showPurchaseDetails(responseData);

        // Обновляем список покупок
        setTimeout(() => {
            if (document.getElementById('purchases-content').classList.contains('active')) {
                loadPurchases();
            }
        }, 1000);

    } catch (error) {
        console.error('Ошибка покупки:', error);
        showNotification(error.message, 'error');

        // При ошибке возвращаем кнопку в исходное состояние
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

// Загрузка покупок
async function loadPurchases() {
    const purchasesList = document.getElementById('purchases-list');
    const errorDiv = document.getElementById('purchases-error');

    purchasesList.innerHTML = `
        <div class="loading">
            <i class="fas fa-spinner"></i>
            <p>Загрузка истории покупок...</p>
        </div>
    `;
    errorDiv.classList.add('d-none');

    try {
        const response = await fetch(`${API_BASE_URL}/purchases?mineOnly=true`, {
            headers: getHeaders()
        });

        if (!response.ok) {
            throw new Error(`Ошибка ${response.status}`);
        }

        const purchases = await response.json();

        // Обновляем Set купленных скинов из истории
        purchases.forEach(p => purchasedSkins.add(p.skinId));

        if (purchases.length === 0) {
            purchasesList.innerHTML = `
                <div class="loading">
                    <i class="fas fa-shopping-bag"></i>
                    <p>У вас пока нет покупок</p>
                </div>
            `;
            return;
        }

        purchasesList.innerHTML = '';

        purchases.forEach((purchase, index) => {
            const item = document.createElement('div');
            item.className = 'purchase-item';
            item.style.animationDelay = `${index * 0.1}s`;

            const date = new Date(purchase.purchasedAtUtc);
            const formattedDate = date.toLocaleDateString('ru-RU', {
                day: 'numeric',
                month: 'long',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            item.innerHTML = `
                <div class="purchase-icon">
                    <i class="fas fa-cube"></i>
                </div>
                <div class="purchase-details">
                    <div class="purchase-name">${purchase.skinName}</div>
                    <div class="purchase-meta">
                        <span><i class="far fa-calendar"></i> ${formattedDate}</span>
                        <span><i class="fas fa-hashtag"></i> ${purchase.id.slice(0, 8)}</span>
                    </div>
                </div>
                <div class="purchase-price">
                    <div class="amount">$${purchase.priceUsdFinal.toFixed(2)}</div>
                    <div class="btc-rate">
                        <i class="fab fa-bitcoin"></i>
                        $${purchase.btcUsdRate.toLocaleString()}
                    </div>
                </div>
            `;

            purchasesList.appendChild(item);
        });

    } catch (error) {
        console.error('Ошибка загрузки покупок:', error);
        errorDiv.querySelector('span').textContent = ' Не удалось загрузить историю покупок';
        errorDiv.classList.remove('d-none');
    }
}

// Тест идемпотентности
window.testIdempotency = async function(skinId) {
    const key = generateIdempotencyKey(skinId);
    console.log('🧪 Тестовый ключ:', key);

    for (let i = 0; i < 3; i++) {
        const response = await fetch(`${API_BASE_URL}/purchases`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-User-Id': USER_ID,
                'Idempotency-Key': key
            },
            body: JSON.stringify({ skinId })
        });
        const data = await response.json();
        console.log(`📦 Запрос ${i + 1}:`, data);
    }
};

// Инициализация
document.addEventListener('DOMContentLoaded', () => {
    loadBtcRate();
    loadSkins();
    setInterval(loadBtcRate, 30000);

    // Загружаем историю покупок в фоне, чтобы узнать, какие скины уже куплены
    setTimeout(() => {
        fetch(`${API_BASE_URL}/purchases?mineOnly=true`, {
            headers: getHeaders()
        })
            .then(r => r.json())
            .then(purchases => {
                purchases.forEach(p => purchasedSkins.add(p.skinId));
                // Обновляем отображение кнопок
                loadSkins();
            })
            .catch(console.error);
    }, 500);
});