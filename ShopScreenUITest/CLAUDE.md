# CLAUDE.md

Este arquivo fornece orientações ao Claude Code (claude.ai/code) ao trabalhar com o código neste repositório.

## Visão Geral do Projeto

Projeto Unity 6 (6000.3.11f1) que implementa um sistema de **Shop UI gerado proceduralmente** usando o UI Toolkit do Unity (UIElements). A loja possui três abas (Ofertas, Dinheiro, Moedas), carteiras com dupla moeda e dois tipos de compra (IAP e Assistir Anúncio).

## Comandos de Desenvolvimento

Este é um projeto Unity — não há build ou testes via CLI. Todo o desenvolvimento ocorre dentro do Unity Editor:

- **Abrir projeto**: Unity Hub → Adicionar projeto em `ShopScreenUITest/`
- **Assistente de configuração**: Menu → `Shop → Setup Shop Scene` (executa `ShopSetupWizard.cs` para montar toda a cena)
- **Rodar testes**: Menu → `Window → General → Test Runner` (usa `com.unity.test-framework`)
- **Verificar compilação**: Qualquer salvamento de script dispara recompilação no Editor

## Arquitetura

### Abstrações Principais

**`UIScreenBase`** (`Assets/Scripts/Utils/UIScreenBase.cs`) — Classe base abstrata para todas as telas. Herde dela para criar novas telas. Hooks de ciclo de vida para sobrescrever:
- `InitializeUIElements()` — cachear referências com `QueryElement<T>()`
- `RegisterCallbacks()` / `UnregisterButtonCallback()` — conectar/desconectar eventos
- `OnScreenEnabled()` / `OnScreenDisabled()` — lógica específica de cada tela

**`UINavigationController`** (`Assets/Scripts/Utils/UINavigationController.cs`) — Navegação estática baseada em eventos. Mantém uma pilha de telas. As telas se registram nos eventos `OnNavigateToScreen` e `OnBackPressed`. Use `NavigateTo()`, `ShowOverlay()` e `GoBack()`.

### Módulo da Loja (`Assets/Scripts/Shop/`)

| Arquivo | Função |
|---------|--------|
| `ShopItemData.cs` | ScriptableObject com dados de cada item (sprite, preço, aba, tipo de compra) |
| `ShopDataConfig.cs` | ScriptableObject de configuração com três arrays: `offersItems`, `moneyItems`, `coinItems` |
| `ShopItemCardBuilder.cs` | Fábrica sem estado — `Build()` instancia um card UXML, vincula os dados e registra o callback de compra |
| `ShopScreenController.cs` | Controlador principal da tela (herda `UIScreenBase`) — troca de abas, carteira e grade de itens |
| `Editor/ShopSetupWizard.cs` | Ferramenta de configuração (somente no Editor) que cria todos os assets e GameObjects do zero |

### Fluxo de Dados

```
ShopDataConfig (ScriptableObject)
    └── ShopItemData[] por aba
            └── ShopItemCardBuilder.Build()
                    └── Instancia ShopItemCard.uxml → preenche visuais → registra callback OnItemPurchased
                            └── ShopScreenController.OnItemPurchased() [estenda aqui para IAP/anúncios reais]
```

### Arquivos do UI Toolkit (`Assets/UI/`)

- `UXML/ShopScreen.uxml` — layout raiz (cabeçalho, carteira, abas, grade com scroll)
- `UXML/ShopItemCard.uxml` — template reutilizável de card
- `USS/ShopVariables.uss` — tokens de design (cores, espaçamentos) — edite aqui para temas
- `USS/ShopScreen.uss` / `USS/ShopItemCard.uss` — estilos dos componentes

### Enums (definidos em `ShopItemData.cs`)

- `ShopTab`: `Offers`, `Money`, `Coins`
- `PurchaseType`: `RealMoney` (IAP), `WatchAd` (anúncio premiado)

## Estendendo a Loja

- **Novo item**: Crie via `Assets → Create → Shop → Shop Item` e atribua ao `ShopDataConfig`
- **Novo tipo de compra**: Adicione ao enum `PurchaseType` e trate em `ShopScreenController.OnItemPurchased()`
- **Nova tela**: Herde de `UIScreenBase` e adicione ao enum `UINavigationController.Screens`
- **Integração IAP/Anúncios**: Implemente dentro de `ShopScreenController.OnItemPurchased()` — o campo `ShopItemData.productId` é o identificador do produto IAP
