# Compatibilidade e prontidao publica

- Suporte do pacote: Windows 11 (build 22000+) desktop, x64 e ARM64. Windows 10, x86 e Windows Server nao estao certificados.
- Distribuicao self-contained: o usuario nao precisa instalar o SDK .NET.
- Preferencias e contas pertencem ao usuario Windows; tokens DPAPI nao devem ser transferidos entre contas ou computadores.
- Localizacao exige permissao do Windows e servico disponivel; cidade manual continua disponivel.
- Google exige consentimento individual. Bloqueio de distribuicao publica: cliente OAuth ainda depende de importacao por usuario e o projeto permanece em teste externo. Nao distribuir google-client.dat pessoal como configuracao compartilhada.
- Startup e solicitado pelo usuario/instalador e pode ser desativado pelo Windows. Ao criar o atalho, migrar somente o Run legado que aponta exatamente para o mesmo executavel.
- Rede indisponivel: manter cache e informar falha; nunca apresentar teste offline como leitura Google real.
- Testes automatizados cobrem recuperacao, encerramento, isolamento, layout, horarios e provedores sinteticos. Nao substituem instalacao MSIX limpa/atualizacao, localizacao real, proxies corporativos, multiplos monitores/DPI e ARM64 em dispositivos de usuarios.
- Publicacao requer configuracao Google pronta, validacao de instalacao e certificacao Microsoft; um bundle gerado nao comprova prontidao universal.
