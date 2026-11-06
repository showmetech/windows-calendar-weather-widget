# Privacidade

O aplicativo mostra previsão do tempo e eventos de calendários escolhidos pelo usuário.

- A localização aproximada do computador é solicitada ao Windows com consentimento. Coordenadas são usadas em consultas ao Open-Meteo; o nome do município pode ser consultado no OpenStreetMap/Nominatim. O usuário pode escolher uma cidade manualmente.
- Calendários Google são consultados somente para leitura após autorização no navegador. Cada instalação precisa de configuração OAuth válida e consentimento próprio. As credenciais de uma pessoa não acompanham o instalador.
- Assinaturas iCalendar consultam o endereço fornecido pelo usuário. Endereços privados e credenciais são protegidos com DPAPI no perfil do Windows.
- Preferências e cópias locais dos eventos ficam no perfil do usuário. Títulos, datas e horários não são enviados a um servidor do desenvolvedor. O app não inclui serviço próprio de telemetria.
- O supervisor acompanha apenas a interface deste aplicativo, registra atividade local e tenta recuperá-la após falhas. Não monitora outros aplicativos.
- O usuário pode desconectar contas, revogar consentimento no provedor e desinstalar o app. Cópias locais de dados podem precisar ser removidas separadamente, conforme o tipo de instalação.

Serviços externos têm suas próprias políticas: [Google](https://policies.google.com/privacy), [Open-Meteo](https://open-meteo.com/en/terms), [OpenStreetMap](https://osmfoundation.org/wiki/Privacy_Policy).

Suporte e solicitações: [issues do projeto](https://github.com/showmetech/windows-calendar-weather-widget/issues). Nunca publique tokens, endereços privados de assinatura, arquivos de credenciais ou dados de calendário em uma issue.
