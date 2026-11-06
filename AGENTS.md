# Entrega e atualizações

Esta pasta é a fonte canônica do Windows Calendar e Weather Widget.

- Toda atualização instalada no desktop deve ter código-fonte e versão correspondentes no repositório showmetech/windows-calendar-weather-widget.
- Antes de instalar uma nova versão, executar o build e os testes, publicar a versão no GitHub e concluir o envio correspondente à Microsoft Store. Conferir separadamente a certificação e a disponibilidade pública na Store.
- Enquanto a conta/publicação na Store estiver bloqueada, preparar e publicar o código e os testes, mas não substituir silenciosamente a instalação local por uma versão divergente. Informar a pendência concreta.
- Seguir docs/PUBLISHING.md. Não declarar sincronização automática somente porque existe um workflow. Validar execução e destinos.
- Nunca publicar dados de usuário, JSON OAuth, tokens, arquivos .dat, certificados privados ou caches. Preservar contas e preferências nas atualizações.
- Preservar o padrão visual da suíte e o ícone único para desktop e bandeja. Validar layout, startup, fechamento intencional e recuperação após falha.
- Usar o navegador do Codex quando uma interface web for necessária.
