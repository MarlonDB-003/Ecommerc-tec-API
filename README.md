# TechWorld API

[![Coverage Report](https://github.com/MarlonDB-003/Ecommerc-tec-API/actions/workflows/coverage.yml/badge.svg)](https://marlondb-003.github.io/Ecommerc-tec-API/)
[![Tests](https://img.shields.io/badge/tests-201%20passing-brightgreen)](https://github.com/MarlonDB-003/Ecommerc-tec-API/actions/workflows/coverage.yml)

REST API de e-commerce para loja de produtos de tecnologia. Construída com .NET 10, Clean Architecture, CQRS e PostgreSQL. Servida em produção através do Caddy (HTTPS automático via Let's Encrypt).

## Funcionalidades

- Autenticação com JWT (registro, login, refresh)
- Catálogo de produtos com filtros, paginação e especificações técnicas
- Upload de imagens via Cloudinary
- Pedidos com múltiplas formas de pagamento e parcelamento
- Endereços com suporte a endereço padrão
- Perfil de usuário com foto de perfil
- Seed automático de admin na primeira inicialização
- 201 testes unitários cobrindo domain, handlers, validators e mappings

## Stack

| Camada | Tecnologia |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| Banco de dados | PostgreSQL 16 + EF Core 10 |
| Autenticação | ASP.NET Core Identity + JWT Bearer |
| CQRS | MediatR 14 |
| Validação | FluentValidation 12 |
| Mapeamento | AutoMapper 16 |
| Imagens | Cloudinary |
| Logging | Serilog |
| Docs | Swagger / OpenAPI |
| Reverse proxy | Caddy 2.8 (TLS automático) |
| Testes | xUnit + Moq + FluentAssertions |

## Arquitetura

```
src/
├── TechWorld.Domain/         # Entidades, enums, regras de negócio
├── TechWorld.Application/    # CQRS handlers, DTOs, validators, mappings
├── TechWorld.Infrastructure/ # EF Core, Identity, JWT, Cloudinary
└── TechWorld.API/            # Controllers, Swagger, DI
tests/
└── TechWorld.UnitTests/      # 201 testes unitários
```

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/register` | Registrar usuário |
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/logout` | Logout |
| GET | `/api/auth/me` | Dados do usuário logado |
| GET | `/api/users/me` | Perfil do usuário |
| PUT | `/api/users/me` | Atualizar perfil |
| POST | `/api/users/me/avatar` | Upload de foto de perfil |
| GET | `/api/products` | Listar produtos (filtros + paginação) |
| GET | `/api/products/featured` | Produtos em destaque |
| GET | `/api/products/{id}` | Detalhes do produto |
| POST | `/api/products` | Criar produto (admin) |
| PUT | `/api/products/{id}` | Atualizar produto (admin) |
| DELETE | `/api/products/{id}` | Remover produto (admin) |
| GET | `/api/orders` | Meus pedidos |
| GET | `/api/orders/{id}` | Detalhes do pedido |
| POST | `/api/orders` | Criar pedido |
| GET | `/api/addresses` | Meus endereços |
| POST | `/api/addresses` | Adicionar endereço |
| PATCH | `/api/addresses/{id}/default` | Definir endereço padrão |
| DELETE | `/api/addresses/{id}` | Remover endereço |
| POST | `/api/images` | Upload de imagem |

## Rodando com Docker

### Pré-requisitos

- Docker e Docker Compose instalados
- Domínio apontando para o servidor (para TLS em produção)

### Configuração

```bash
cp .env.example .env
```

Edite o `.env` com seus valores:

```env
DB_PASSWORD=sua_senha_segura
JWT_SECRET=sua_chave_jwt_com_minimo_32_caracteres
SEED_ADMIN_PASSWORD=senha_do_admin
DOMAIN=seudominio.com
CORS_ORIGIN=https://seudominio.com

# Cloudinary (opcional — para upload de imagens)
CLOUDINARY_CLOUD_NAME=
CLOUDINARY_API_KEY=
CLOUDINARY_API_SECRET=
```

### Subir a aplicação

```bash
docker compose up -d
```

A aplicação ficará disponível em `https://seudominio.com`. O Caddy obtém o certificado TLS automaticamente via Let's Encrypt.

### Serviços Docker

| Serviço | Descrição | Porta pública |
|---|---|---|
| `caddy` | Reverse proxy com TLS | 80, 443 |
| `frontend` | Aplicação React | — (interno) |
| `api` | API .NET | — (interno) |
| `db` | PostgreSQL 16 | — (interno) |

## Desenvolvimento local

### Pré-requisitos

- .NET 10 SDK
- Docker (para o banco de dados)
- PostgreSQL rodando em `localhost:5432`

### Opção A — banco via Docker

```bash
# Expõe o postgres na porta 5432 do host
docker compose up db -d

# Rode a API
dotnet run --project src/TechWorld.API
```

> O arquivo `docker-compose.override.yml` já expõe a porta do banco para o host.

### Opção B — PostgreSQL local

Configure a connection string em `src/TechWorld.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=techworlddb;Username=postgres;Password=sua_senha"
  }
}
```

A API aplica as migrations e faz o seed do admin automaticamente na inicialização.

### Rodando os testes

```bash
dotnet test tests/TechWorld.UnitTests
```

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `DB_PASSWORD` | Sim | Senha do PostgreSQL |
| `JWT_SECRET` | Sim | Chave de assinatura JWT (mín. 32 chars) |
| `SEED_ADMIN_PASSWORD` | Sim | Senha do admin criado no seed |
| `DOMAIN` | Sim (produção) | Domínio para o Caddy obter TLS |
| `CORS_ORIGIN` | Não | Origem permitida no CORS (padrão: `http://localhost`) |
| `CLOUDINARY_CLOUD_NAME` | Não | Nome do cloud no Cloudinary |
| `CLOUDINARY_API_KEY` | Não | API Key do Cloudinary |
| `CLOUDINARY_API_SECRET` | Não | API Secret do Cloudinary |
| `ASPNETCORE_ENVIRONMENT` | Não | Ambiente (`Production` por padrão) |

## Categorias de produtos

`Todos` · `Smartphones` · `Gaming` · `Consoles` · `Componentes` · `Computadores`

## Formas de pagamento

`CreditCard` · `DebitCard` · `Pix` · `Boleto`

## Status de pedido

`Pending` → `Processing` → `Shipped` → `Delivered` · `Cancelled`
