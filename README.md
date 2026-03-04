## Основное

Четыре центральных элемента: запуск сессии и сцена, игровая логика и сервисы, сетевой контекст, статы и модификаторы.

### Game

**Файл:** `Code/Game/Logic/Game.cs`

Точка входа в сетевую игру. `MonoBehaviour`, реализует `INetworkRunnerCallbacks`.

- **Старт:** `StartGame(sessionName)` — создаёт `NetworkRunner`, подключается (AutoHostOrClient), передаёт `ConnectionToken` (PlayerId, SkinId), загружает сцену Game, вызывает `GameLogic.PrepareGame`, подписывает уже активных игроков, кидает `OnGameStart`.
- **Стоп:** `EndGame()` — Shutdown раннера, загрузка сцены Lobby.
- **Host migration:** при `OnHostMigration` перезапускает раннер с `HostMigrationToken`, догружает сцену, снова вызывает `PrepareGame` и при необходимости `HostMigrationResume`.
- Держит ссылку на `Runner`; через `GameServicesWrapper` отдаёт доступ к `GameLogic`.

---

### GameLogic

**Файл:** `Code/Game/Logic/GameLogic.cs`

Центр игровой логики. `SimulationBehaviour`, синглтон `GameLogic.Instance`.

- **Подготовка сессии:** `PrepareGame(runner)` — добавляет себя в раннер, инициализирует сетевые состояния (`InitNetworkServicesStates`), создаёт `PlayerConnectionService` и `HostMigrationService`, восстанавливает колбэки и симуляционные объекты, инициализирует `PlayerSetupService`.
- **Игроки:** создаёт игрока через `PlayerFactory` по `PlayerRef` + `PlayerConnectionData`, уничтожает через `PlayerProvider` + `Runner.Despawn`; при смерти игрока вызывает `PlayerConnectionService.Ban`.
- **Регистрация:** ведёт списки `INetworkRunnerCallbacks` и `SimulationBehaviour` для восстановления после миграции; регистрирует игроков в `PlayerProvider` и подписывается на их `Death.OnDie`.
- **Связь с контекстом:** `Init(GameNetworkContext)` сохраняет контекст; при подготовке ждёт спавна/валидности `GameNetworkContext` и его состояний, затем вызывает `NetworkGameContext.InitStates()` и инжектит в контекст DI.

Публичные сервисы: `PlayerConnectionService`, `HostMigrationService`.

---

### GameNetworkContext

**Файл:** `Code/Infrastructure/Installers/GameNetworkContext.cs`

Сетевой контекст одной игровой сессии. `NetworkBehaviour`, спавнится на сервере один раз.

- **Данные:** `BlackListId` (забаненные игроки), `EnemiesProviderState`, `PlayerProviderState` — сетевые состояния провайдеров врагов и игроков.
- **Инициализация:** в `Spawned` вызывает `GameLogic.Init(this)`. Метод `InitStates()` ждёт валидности объекта и состояний, затем резолвит из контейнера `EnemiesProvider` и `PlayerProvider` и вызывает у них `Init(EnemiesProviderState)` / `Init(PlayerProviderState)`.
- Через DI получает `DiContainer` для резолва провайдеров при инициализации.

Игра ждёт появления и валидности `GameNetworkContext` перед тем, как поднимать `PlayerConnectionService` и остальную логику.

---

### Stats и модификаторы

**Stats** — контейнер всех статов сущности (игрок/враг). **Модификаторы** — способ менять статы (баффы, предметы, уровни) через типизированные дельты и операции (Add/Multiply).

#### Stats

**Файл:** `Code/Infrastructure/Stats/Stats.cs`

`NetworkBehaviour`, висит на игроке (или другой сущности).

- **Статы:** `Health`, `Attack`, `Movement`, `Level` — ссылки на компоненты; в `Start` регистрируются в словарях по типам (`IStat`, для модифицируемых — ещё `IModifiableStat<TState>`).
- **Модификаторы:** `ApplyModifier(IStatModifier modifier)` — вызывается у модификатора `Apply(this)`, затем модификатор сохраняется в списке.
- **Доступ:** `GetModifiableStat<TState>()` — возвращает стат, модифицируемый состоянием типа `TState` (например `HealthStatState`).

#### Цепочка модификатора

1. **IStatModifier** (`Modifiers/IStatModifier.cs`) — один метод `Apply(Stats stats)`. Модификатор сам решает, какой стат менять.
2. **StatModifier&lt;TStat, TState&gt;** (`Modifiers/StatModifier.cs`) — реализация: хранит дельту `TState`, тип операции и `IModifierApplier`; в `Apply` берёт у `Stats` стат по `GetModifiableStat<TState>()` и вызывает у апплаера `Apply(stat, modifier, operationType)`.
3. **IModifierApplier** (`Modifiers/IModifierApplier.cs`) — применяет к стату пару (модификатор, тип операции).
4. **DefaultModifierApplier** (`Modifiers/DefaultModifierApplier.cs`) — переводит `ModifierOperationType` в бинарную операцию: Add → `(a,b) => a+b`, Multiply → `(a,b) => a*b`, и вызывает у стата `Apply(in modifier, operation)`.
5. **IModifiableStat&lt;TStatState&gt;** (`Modifiers/IModifiableStat.cs`) — у стата метод `Apply(in TStatState modifier, BinaryStatOperation operation)`; конкретные статы (Health, Attack, Movement) применяют операцию к полям своего состояния.
