# Brasfoot Godot

Jogo de manager de futebol feito em Godot 4.7 com C# (.NET 8). A temporada começa em 2026. Você escolhe um clube, treina o elenco e conduz a temporada: campeonatos estaduais, ligas nacionais, copa e competição continental.

## O que dá para fazer

- Começar uma carreira, salvar e carregar.
- Acompanhar o escritório do clube: calendário, tabela, artilharia, elenco, finanças e mercado.
- Assistir a partida narrada, com gols, cartões e o placar final.
- Encerrar a temporada e seguir para o ano seguinte. Confiança baixa pode terminar em demissão.
- Editar elencos, clubes e jogadores do catálogo.
- Ajustar idioma (português, inglês ou espanhol), volume e tamanho da janela.

A janela abre em 1366×768. O idioma padrão é português.

## Como abrir

É preciso o Godot 4.7.2 com suporte a .NET e o SDK do .NET 8.

1. Abra esta pasta no editor Godot.
2. Rode a cena principal `scenes/Boot.tscn`.

O jogo lê o catálogo em `catalog/brasfoot.res`: clubes, ligas, estaduais, países e as listas de nomes. Carreiras ficam em `user://saves` e as preferências em `user://settings.res`. Os três arquivos são recursos Godot comprimidos, com campos em inglês e versão de formato 2. Um arquivo gravado antes dessa versão não abre.

## Pastas

| Pasta | Conteúdo |
| --- | --- |
| `scenes/` | Cenas: menu, carreira, partida, temporada, editor |
| `scripts/screens/` | Telas |
| `scripts/domain/` | Temporada, partida, regras e modelos |
| `scripts/data/` | Catálogo, saves e caminhos |
| `scripts/presentation/` | Bandeiras, tradutor, ícones, tema visual em código |
| `scripts/components/` | Grade, campo, linha de elenco |
| `scripts/autoload/` | `Catalog`, `Session` e `Preferences` |
| `art/clubs/` | Escudos e camisas (`crests`, `crests_small`, `shirts`, `shirts_away`, `shirts_third`) |
| `art/national/` | Camisas de seleções |
| `art/trophies/` | Troféus |
| `art/photos/` | Fotos |
| `art/sounds/` | Sons da partida |
| `art/flags/` | Bandeiras (`countries`, `countries_large`, `states`) |
| `art/icons/` | Ícones da interface |
| `art/ui/` | Cores e camisas lisas |
| `art/pitch/` | Campo e pênalti |
| `art/menu/` | Fundo e ícones do menu |
| `translations/` | Textos em CSV (`brasfoot.csv`) |
| `theme/` | Tema da interface |
| `catalog/` | Catálogo empacotado |

## Código

Namespace `Brasfoot`. Pastas, classes, métodos, nomes de nós e campos gravados estão em inglês.

Os lances da partida usam `goal`, `yellow`, `red`, `injury`, `halftime` e `full_time`. As competições usam `groups`, `league`, `league_knockout` e `knockout`. As táticas gravadas são `balanced`, `all_out_attack` e `counter`. Os textos que aparecem na tela vêm do CSV de tradução.

Imagens, arquivos `.import` e `.uid` entram no Git. A pasta `.godot/`, os `.translation` gerados e os prints de verificação (`art/verificacao_*.png`) ficam de fora.
