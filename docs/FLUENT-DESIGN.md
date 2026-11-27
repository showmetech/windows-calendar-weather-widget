# Interface Fluent 2

O widget usa WPF e os recursos Fluent do .NET. Os padrões de componentes React são referências de comportamento; não são uma dependência web nem uma afirmação de uso de WinUI 3.

## Organização

- **Widget:** tema, opacidade e inicialização. Frequência das atualizações fica em uma tela secundária, com retorno explícito.
- **Clima:** localização atual e modo automático. Pesquisa manual é uma etapa separada, com campo rotulado, confirmação e retorno.
- **Agenda:** conta Google, assinatura/arquivo iCalendar e seleção de calendários. OAuth normal nunca solicita JSON de desenvolvedor. Cada pessoa autoriza sua própria conta.

As três categorias usam TabControl nativo, com seleção e navegação por teclado. Contas e calendários são paginados para manter os controles acessíveis sem rolagem nas configurações. Não há rotação automática de páginas.

## Apresentação e acessibilidade

Superfícies sólidas, opacas por padrão, com contorno e raios consistentes; cor de destaque do Windows; texto sobre destaque escolhido por contraste relativo. Texto comum de 14 DIPs, legendas de 12, título de 20. Títulos em sentence case. Ícones têm nomes acessíveis; botões mantêm foco visível e ação por teclado. Estados de falha incluem uma ação e preservam dados anteriores.

As transições de página duram 120 ms e podem ser desativadas em Reduzir animações; as preferências de movimento do Windows e alto contraste também são respeitadas. Animação nunca é a única forma de comunicar o estado.

O painel mantém a largura do widget e fica acima dele quando há espaço. Em áreas úteis pequenas, ele ocupa uma posição dentro da tela e pode cobrir o widget intencionalmente. Os cálculos consideram DPI e monitores com origem negativa. Isso não comprova teste físico em todos os monitores possíveis.

Padrões iniciais: opacidade de 100%, localização automática, iniciar com Windows e intervalos de cinco minutos. Preferências existentes são preservadas.

## Verificação

`--self-test <pasta-isolada> --offline` verifica os cinco painéis internos, busca manual/retorno e assinatura, temas claro/escuro, três larguras (320/360/540) e duas alturas (420/460), paginação de 19 calendários, contraste em 4.096 cores de destaque, redução de movimento e limites geométricos em quatro escalas. O teste não altera escala/tema global do Windows nem autentica uma conta real.

Além do teste automatizado, a revisão nativa exige cliques, foco, teclado, abertura/fechamento, animações e inspeção das imagens. A conexão real, instalação, startup no próximo login e certificação Store têm evidências próprias; um self-test aprovado não substitui essas etapas.

## Fontes

[Fundamentos](https://fluent2.microsoft.design/design-principles), [layout](https://fluent2.microsoft.design/layout), [tipografia](https://fluent2.microsoft.design/typography), [cor](https://fluent2.microsoft.design/color), [materiais](https://fluent2.microsoft.design/material), [movimento](https://fluent2.microsoft.design/motion), [acessibilidade](https://fluent2.microsoft.design/accessibility), [guias](https://fluent2.microsoft.design/components/web/react/core/tablist/usage), [switch](https://fluent2.microsoft.design/components/web/react/core/switch/usage), [campos](https://fluent2.microsoft.design/components/web/react/core/field/usage).

O índice público inclui páginas restritas à Microsoft e componentes de outras plataformas/IA. Não se declara acesso a esse conteúdo, nem se adicionam recursos que não atendam ao propósito do widget.
