# Checklist de Entrega - Tech Challenge Fase 3

## ✅ Infraestrutura

- [x] Kong API Gateway configurado
- [x] Prometheus instalado e configurado
- [x] Grafana com datasources
- [x] MongoDB inicializado
- [x] Redis ativo
- [x] RabbitMQ para fila de mensagens
- [x] Docker Compose funcionando

## ✅ API Gateway (Kong)

- [ ] Routes configuradas para UsersAPI
- [ ] Routes configuradas para CatalogAPI
- [ ] Routes configuradas para PaymentsAPI
- [ ] Routes configuradas para OrchestrationAPI
- [ ] Plugin JWT ativo
- [ ] Rate limiting configurado
- [ ] Logging habilitado

## ✅ Observabilidade

### Prometheus
- [x] Configuração de scrape jobs
- [x] Alert rules definidas
- [x] Retention policy configurada

### Grafana
- [ ] Dashboard API Gateway
- [ ] Dashboard API Latency
- [ ] Dashboard Error Rate
- [ ] Dashboard Throughput
- [ ] Dashboard System Metrics
- [ ] Alertas configuradas

## ✅ Persistência

### MongoDB
- [x] Inicialização com collections
- [ ] Índices criados
- [ ] Backup configurado
- [ ] Replicação (se aplicável)

### Redis
- [x] Servidor rodando
- [ ] Persistência (AOF/RDB)
- [ ] Redis Sentinel (alta disponibilidade)

## ✅ Microsserviços - Instrumentação

- [ ] fcg-users-api: Prometheus metrics
- [ ] fcg-catalog-api: Prometheus metrics
- [ ] fcg-payments-api: Prometheus metrics
- [ ] fcg-orchestration: Prometheus metrics
- [ ] Logs enviados para stdout (Docker)

## ✅ Serverless

- [ ] Azure Function criada
- [ ] NotificationsAPI migrada
- [ ] Trigger por RabbitMQ configurado
- [ ] IaC (Terraform/ARM/Bicep)
- [ ] Deploy em ambiente Azure

## ✅ Código-Fonte

- [x] Repositório fcg-fase-3 com commits
- [ ] Submodules atualizados
- [ ] README.md completo
- [ ] docker-compose.yml atualizado
- [ ] Configuração do Kong (YAML/JSON)
- [ ] Código de cada microsserviço com metricas

## ✅ Testes

- [ ] Fluxo de login funciona
- [ ] Listagem de games retorna dados
- [ ] Criação de pedido persiste em MongoDB
- [ ] Cache em Redis funciona
- [ ] Metricas aparecem no Prometheus
- [ ] Grafana mostra dashboards
- [ ] Azure Function dispara e envia notificação
- [ ] Kong roteia requisições corretamente

## ✅ Documentação

- [x] README.md com arquitetura
- [x] QUICK_START.md com instruções
- [ ] Guia de testes (teste cada funcionalidade)
- [ ] Documentação de deploy
- [ ] Configuração de secrets (Kubernetes)

## ✅ Vídeo de Demonstração (até 20 min)

- [ ] Iniciar docker-compose
- [ ] Demonstrar Kong Admin UI
- [ ] Fazer requisição via Kong (mostrando roteamento)
- [ ] Mostrar erro intencional e ver taxa de erro no Grafana
- [ ] Exibir dashboard Grafana com métricas
- [ ] Demonstrar MongoDB com dados
- [ ] Mostrar logs da Azure Function
- [ ] Explicar fluxo completo de uma compra

## ✅ Relatório de Entrega

- [ ] Nome do grupo
- [ ] Nomes dos participantes
- [ ] Usernames no Discord
- [ ] Link da documentação (GitHub)
- [ ] Link do repositório (GitHub)
- [ ] Link do vídeo (YouTube/Drive)
- [ ] Descrição da arquitetura
- [ ] Ferramentas utilizadas
- [ ] Desafios enfrentados
- [ ] Lições aprendidas

## 📊 Status Geral

| Componente | Status | Responsável |
|------------|--------|-------------|
| Infrastructure | ✅ Completo | Lavinia |
| API Gateway | ⏳ Em Progresso | - |
| Observabilidade | ✅ Completo | - |
| Microsserviços | ⏳ Em Progresso | - |
| Serverless | ⏳ Em Progresso | - |
| Testes | ⏳ Em Progresso | - |
| Documentação | ⏳ Em Progresso | - |
| Vídeo | ⏳ Pendente | - |
| Relatório | ⏳ Pendente | - |

---

**Última atualização:** 2026-09-15
