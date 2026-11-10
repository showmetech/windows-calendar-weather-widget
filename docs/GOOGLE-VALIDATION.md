# Validação real do consentimento Google

O painel em `tools/GoogleConsentValidation` chama os mesmos métodos `Services.Connect` e `Services.Calendar` usados pelo widget. Ele não simula aprovação, não reutiliza cookies do navegador e não copia a autorização do app instalado.

O primeiro consentimento requer uma ação do titular da conta no navegador externo aberto pelo próprio aplicativo. Não envie senhas, códigos, JSON OAuth ou arquivos `.dat` em issues ou conversas.

## Execução opt-in em Windows x64

1. Compile o aplicativo normalmente e compile o painel: `dotnet publish tools/GoogleConsentValidation/GoogleConsentValidation.csproj -c Release -r win-x64 --self-contained true -o .artifacts/google-validation`.
2. Escolha uma pasta privada fora do repositório cujo nome final seja `public-oauth-profile`. Apesar do nome, ela contém dados privados e não deve ser compartilhada.
3. Importe um cliente OAuth Desktop válido com o executável principal: `WindowsCalendarWeatherWidget.exe --data-root "CAMINHO_PRIVADO/public-oauth-profile" --import-client "CLIENTE_PRIVADO.json"`. Não copie `google-client.dat` de outra pessoa ou de outro computador.
4. Execute o painel compilado: `GoogleConsentValidation.exe "CAMINHO_PRIVADO/public-oauth-profile" --self-test --inspect`. O marcador `--self-test` mantém desativadas as escritas de inicialização e a detecção automática de localização quando o widget de validação for aberto. O painel em si não escreve no startup do Windows.
5. Clique em **Conectar Google — somente leitura** e conclua a autorização da própria conta no navegador. No modo de teste do Google, essa conta precisa estar cadastrada como usuária de teste no projeto.
6. Aguarde o resultado. `CalendarReadVerified: true` em `consent-validation-status.json` só é gravado após troca real de tokens e leitura real dos calendários. O relatório contém contagens e estado, sem tokens, emails ou títulos de eventos.
7. **Abrir widget com os dados deste teste** permite inspecionar o widget original com essa conta isolada. **Concluir e fechar teste** encerra o painel; suas credenciais locais de teste permanecem protegidas por DPAPI no perfil escolhido.

## Limites da evidência

Compilar este painel não comprova consentimento, leitura de calendários, verificação Google ou publicação na Store. O CI apenas compila o painel e mantém testes automatizados offline separados. A validação real depende do titular da conta.

O relatório pode conter `AccountCount: 0` enquanto aguarda o consentimento. Se a conexão for cancelada ou falhar, não declare sucesso. Para uma demonstração pública ao Google, prepare um calendário sem dados pessoais e revise o vídeo antes do envio. Não substitua uma demonstração real por uma animação ou dados sintéticos apresentados como reais.
