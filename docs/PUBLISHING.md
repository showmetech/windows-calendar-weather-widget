# Fluxo único de atualização

1. Alterar e testar este código-fonte canônico. Revisar e criar um commit, com aumento da versão em src/WindowsCalendarWeatherWidget.csproj.
2. Executar `python scripts/release.py publish` em um checkout autenticado. O comando publica o commit e a tag; não instala uma versão nova no desktop.
3. GitHub Actions compila x64/ARM64, executa testes isolados, publica arquivos versionados no GitHub e submete o MSIX à Microsoft Store.
4. Verificar a certificação e a versão publicamente disponível no Partner Center. Submissão aceita não equivale a publicação aprovada.
5. Instalar somente o artefato correspondente à mesma versão/commit. Usuários da Store recebem atualizações pelo Windows, sujeito às configurações do sistema.

Uma alteração arbitrária nos binários locais não pode ser automaticamente convertida em código-fonte no GitHub. Por isso futuras atualizações devem passar por este fluxo. Não substituir o app instalado diretamente com builds de trabalho e declarar a Store sincronizada.

## Configuração da Microsoft Store

É necessária uma conta de desenvolvedor Windows ativa. Reservar o nome do app e copiar a identidade exata fornecida pelo Partner Center para `store/identity.json` (fora do git), seguindo identity.example.json. Não inventar Publisher ou Product ID.

Variáveis GitHub: STORE_IDENTITY_NAME, STORE_PUBLISHER, STORE_PUBLISHER_DISPLAY_NAME, STORE_PRODUCT_ID.

Segredos GitHub: AZURE_AD_TENANT_ID, SELLER_ID, AZURE_AD_APPLICATION_CLIENT_ID, AZURE_AD_APPLICATION_SECRET. A aplicação Entra precisa ser autorizada no Partner Center. Sua criação/permissões devem ser aprovadas pelo proprietário. Nunca colar segredos em arquivos versionados, chat ou logs.

O primeiro envio exige cadastro, dados de listagem, privacidade, classificação etária e certificação. A automação oficial de atualizações pressupõe um produto já publicado. O job da Store falha explicitamente se a configuração estiver incompleta; não marca publicação como concluída.

## Validação

`python scripts/release.py build` gera ZIPs independentes x64/ARM64 e executa testes x64 sem acessar contas reais. `python scripts/release.py msix` exige identidade real e Windows SDK (MakeAppx). O bundle de submissão não é assinado com certificado de desenvolvimento; a Store assina o pacote aprovado. Não instalar certificado de teste em computadores de usuários.

Também são necessários testes de instalação/atualização MSIX, consentimento/localização, conta Google de um novo usuário, ARM64 físico e múltiplos monitores/DPI. Um build bem-sucedido não substitui esses testes.

`python scripts/release.py msix-test` valida os pacotes x64/ARM64, o bundle e o contêiner de upload com MakeAppx e uma identidade explicitamente fictícia, sem assinatura nem instalação. Os pacotes ficam em `.artifacts/packaging-test`, fora dos artefatos de release. O CI executa esse teste mesmo antes da aprovação empresarial, restaurando as ferramentas oficiais com `dotnet restore tools/Packaging/Packaging.csproj`.

O WACK está descontinuado e é uma verificação local opcional; a certificação oficial é feita no Partner Center. [Documentação Microsoft](https://learn.microsoft.com/windows/msix/package/packaging-uwp-apps#validate-your-app-package).

O job `arm64-runtime` usa `windows-11-arm`, executa o ZIP ARM64 produzido pelo mesmo build e exige que o próprio processo reporte arquitetura Arm64 e testes aprovados. Ele não autentica contas reais, concede localização nem substitui testes visuais e de instalação em computadores de usuários. [Runners oficiais](https://docs.github.com/en/actions/reference/runners/github-hosted-runners).

## Bloqueios externos e resolução

Usar sempre o produto e o publicador empresarial existentes. Aprovação da conta, upload, certificação e disponibilidade pública são etapas diferentes. Uma falha de credenciais da API Store ocorre antes do envio e não autoriza criar outra conta ou produto.

## Google Calendar para distribuição

O produto usa seu cliente OAuth Desktop dedicado. O build recebe a configuração pelos metadados `CalendarGoogleDesktopClientId` e `CalendarGoogleDesktopClientSecret`; em CI, o ID vem da variável do repositório e a configuração complementar do secret existente. Não colocar JSONs locais, tokens de usuários ou perfis no Git. A configuração pública de um cliente Desktop não equivale a uma credencial de conta autorizada.

O botão normal Conectar conta Google inicia o OAuth legítimo com PKCE e retorno local no navegador. Ele nunca solicita um arquivo do usuário. Se a configuração do produto estiver ausente, o app informa um erro do desenvolvedor; isso deve ser reparado no build, e não transferido ao usuário como uma instrução para procurar um JSON. Cada usuário autoriza sua própria conta; não copiar sessões entre máquinas.

A verificação pública do Google e o teste real de consentimento continuam independentes dos testes offline e da existência do cliente. ICS e clima não usam OAuth Google. Não anunciar a conexão pronta em uma instalação nova antes de validar autorização, leitura e persistência reais.

## Builds em pastas sincronizadas

O código pode ficar no Drive; configure `SMT_CALENDAR_BUILD_ROOT` para uma pasta local de caches MSBuild, `SMT_CALENDAR_STAGE_ROOT` para staging local e `SMT_CALENDAR_ARTIFACTS_ROOT` para saída de pacotes local. Sem essas variáveis, os caminhos do CI permanecem os mesmos. Isso evita locks de arquivos mapeados no CreateAppHost sem mover a fonte canônica ou incluir caches no repositório. Copiar somente artefatos aprovados e hashes para a gestão institucional.

Fontes oficiais: [GitHub Actions e Microsoft Store](https://learn.microsoft.com/windows/apps/publish/msstore-dev-cli/github-actions), [distribuição Win32](https://learn.microsoft.com/windows/apps/distribute-through-store/how-to-distribute-your-win32-app-through-microsoft-store), [StartupTask](https://learn.microsoft.com/uwp/api/windows.applicationmodel.startuptask).
