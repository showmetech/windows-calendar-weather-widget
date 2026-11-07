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

## Bloqueios externos e resolução

A revisão empresarial manual pode levar de 2 a 5 dias úteis. Enquanto estiver em análise e sem pedido de documentos, aguardar a Microsoft; se houver ação necessária, cumprir a solicitação exibida. Para falhas ou atraso além da janela indicada, usar o suporte oficial. Não reiniciar o cadastro nem criar inscrições duplicadas. [Fluxo empresarial](https://learn.microsoft.com/windows/apps/publish/whats-new-company-developer).

O cliente OAuth atual pertence a um projeto compartilhado com outros aplicativos. Não mudar sua marca, remover escopos ou submeter todos os serviços à verificação em nome apenas deste widget. A distribuição pública deve usar um projeto dedicado, somente com os escopos de leitura de calendários e eventos, políticas específicas e demonstração real do consentimento. Criar o projeto não comprova aprovação Google nem autoriza transferir tokens existentes.

## Google Calendar para distribuição

A configuração desta máquina é protegida por usuário e não pode ser copiada. Antes de uma distribuição de uso geral, provisionar um cliente OAuth Desktop próprio para o produto e concluir a verificação de consentimento/escopos exigida pelo Google, ou orientar cada usuário a importar seu próprio JSON. ICS e clima não usam OAuth Google. Não anunciar conexão Google pronta sem configuração e teste de uma instalação nova.

Fontes oficiais: [GitHub Actions e Microsoft Store](https://learn.microsoft.com/windows/apps/publish/msstore-dev-cli/github-actions), [distribuição Win32](https://learn.microsoft.com/windows/apps/distribute-through-store/how-to-distribute-your-win32-app-through-microsoft-store), [StartupTask](https://learn.microsoft.com/uwp/api/windows.applicationmodel.startuptask).
