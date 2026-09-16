# FCG — Tech Challenge Fase 3

Este repositório foi usado como monorepo de desenvolvimento da Fase 3, mas o código foi migrado para os repositórios oficiais de cada componente, seguindo a estrutura pedida no enunciado (repositórios dos microsserviços atualizados + repositório de orquestração + repositório próprio da função serverless).

## Repositórios

| Componente | Repositório |
|---|---|
| Orquestração (docker-compose, manifestos K8s, Kong, Prometheus/Grafana) | [fcg-orchestration](https://github.com/laviniaptrabuco/fcg-orchestration) |
| UsersAPI (SQL Server + MongoDB + Redis + Prometheus) | [fcg-users-api](https://github.com/laviniaptrabuco/fcg-users-api) |
| CatalogAPI (SQL Server + MongoDB + Redis + Prometheus) | [fcg-catalog-api](https://github.com/laviniaptrabuco/fcg-catalog-api) |
| PaymentsAPI (MongoDB + Prometheus) | [fcg-payments-api](https://github.com/laviniaptrabuco/fcg-payments-api) |
| Notifications (Azure Function, RabbitMQTrigger) | [fcg-notifications-serverless](https://github.com/laviniaptrabuco/fcg-notifications-serverless) |

O guia central de como subir o ambiente completo (Docker Compose ou Kubernetes) está no README do [fcg-orchestration](https://github.com/laviniaptrabuco/fcg-orchestration).

Cada repositório de microsserviço e o de orquestração também tem a tag `fase-2`, com o estado exato da fase anterior (sem Kong, observabilidade ou MongoDB), para quem quiser rodar aquela versão.
