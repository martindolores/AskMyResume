# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A RAG (retrieval-augmented generation) chatbot that answers questions about the repo owner's resume/portfolio. The chatbot itself isn't the point — it's a vehicle to learn and be able to defend, in an interview, four skills: Bicep, Azure Container Apps, applied AI development (Semantic Kernel/RAG), and Kubernetes (stretch goal).

Full architecture, cost guardrails, build plan, and open decisions live in `README.md` — read it before making any structural changes. Work is tracked as GitHub issues (PR-0 through PR-12) on the [project board](https://github.com/users/martindolores/projects/2); each issue is a single PR-sized chunk of the build plan in `README.md`.

## Hard constraint: this must cost $0

This is a hobby project with no budget. Every architecture decision is chosen to stay inside always-free tiers (GitHub Models for LLM calls, Container Apps Consumption plan, `kind`/`minikube` for Kubernetes practice). Do not introduce anything that bills by default — see the cost guardrails table in `README.md` before adding infrastructure or switching LLM providers. Real AKS is the only paid resource, and only via the Azure free trial credit, torn down the same day it's used.

## Commands

```bash
# build
dotnet build

# run the API locally (from repo root)
dotnet run --project src/AskMyResume.Api
```

There is no test project yet — none has been scaffolded as of this writing.

## Architecture

Currently a single ASP.NET Core minimal API project, `src/AskMyResume.Api` (targets net10.0), referenced from the root `AskMyResume.slnx` solution file. Per `README.md`, the intended end state is:

```
src/AskMyResume.Api    ASP.NET Core minimal API, one POST /chat endpoint
                        Semantic Kernel embeds resume/portfolio text, retrieves
                        relevant chunks, calls an LLM via GitHub Models, returns
                        an answer

infra/main.bicep        Container App, Container App Environment, Log Analytics
                         workspace, Key Vault (not yet created)

Dockerfile               containerizes the API (not yet created)

.github/workflows/       build image, push to GHCR, deploy via Bicep (not yet created)

k8s/                      deployment.yaml, service.yaml, configmap.yaml — stretch
                          goal, tested against kind/minikube, deployed to a
                          throwaway AKS cluster once (not yet created)
```

There is no vector database — retrieval is in-memory cosine similarity over a small set of plain-text resume/portfolio files, which is sufficient RAG at this scale (see "Open decisions" in `README.md`).

There is no frontend. The only interface is the `POST /chat` HTTP endpoint.
