# Ask My Resume

A small RAG (retrieval-augmented generation) chatbot that answers questions about Martin's own experience, by retrieving relevant chunks from his resume/portfolio content and feeding them to an LLM. The point isn't the chatbot itself — it's a vehicle to genuinely learn and be able to defend, in a job interview, four skills identified as missing from his CV:

- **Bicep** (Azure infrastructure-as-code)
- **Containers on Azure** (Container Apps, optionally AKS)
- **Applied AI development** (Semantic Kernel, RAG) — not just using AI coding assistants, but building AI features
- **Kubernetes** (stretch goal, via local `kind`/`minikube` + one short AKS excursion)

Work is tracked as issues on the [AskMyResume project board](https://github.com/users/martindolores/projects/2), one issue per PR-sized chunk (PR-0 through PR-12).

## Hard constraint: this must cost $0

This is a hobby project with no budget. Every architecture decision below was chosen specifically to stay inside always-free tiers. **Do not deviate from the cost guardrails below without re-checking current Azure pricing** — free tier terms change.

### Cost guardrails (read before provisioning anything)

| Resource | Why it's free | Guardrail |
|---|---|---|
| Azure Container Apps | Consumption plan free tier: 180,000 vCPU-seconds/month + scale-to-zero | Low-traffic personal project stays inside this. Don't switch to Dedicated workload profiles (billed per node-hour). |
| Azure Functions (alternative to Container Apps) | 1M executions/month always free | Fine as a fallback if Container Apps ever looks like it'll exceed free tier. |
| LLM calls | Use **GitHub Models** (github.com/marketplace/models) free tier — OpenAI-compatible, works with Semantic Kernel | Do NOT wire up paid Azure OpenAI for routine use. Only touch Azure OpenAI if doing a short, deliberate paid experiment, and know the cost before running it. |
| Bicep | Free — it's just a deployment template | You only pay for what it provisions, so keep provisioned resources inside this table. |
| Kubernetes practice | `kind` or `minikube` — a real K8s cluster running locally on the laptop | Unlimited, free, no cloud involved. Do all manifest iteration here. |
| Real AKS (stretch goal only) | Control plane is free; **worker node VMs bill by the hour** | Only spin up using the $200/30-day Azure free trial credit. Deploy, capture screenshots + `kubectl` output for the portfolio, then `az group delete` the same day. **Never leave an AKS cluster running unattended.** |

If in doubt about whether something costs money: check the Azure Pricing Calculator before running `az deployment` or `az aks create`, not after.

## Architecture

```
GitHub repo
  │
  ├─ src/AskMyResume.Api          ASP.NET Core minimal API, one POST /chat endpoint
  │     └─ Semantic Kernel        embeds resume/portfolio text, retrieves relevant
  │                               chunks, calls LLM via GitHub Models, returns answer
  │
  ├─ infra/main.bicep             defines: Container App, Container App Environment,
  │                               Log Analytics workspace, Key Vault (for the GitHub
  │                               Models token / any secrets)
  │
  ├─ Dockerfile                   containerizes the API
  │
  ├─ .github/workflows/deploy.yml builds the image, pushes to GHCR (free for public
  │                               repos), runs `az deployment group create` with the
  │                               Bicep file to deploy/update the Container App
  │
  └─ k8s/ (stretch goal)
        deployment.yaml, service.yaml, configmap.yaml — tested against kind/minikube
        locally; deployed to a throwaway AKS cluster once, then torn down
```

### Why these choices specifically
- **ASP.NET Core**: already Martin's daily stack (Interfi) — no new language to learn, keeps focus on the infra/AI skills that are actually new.
- **Semantic Kernel over LangChain**: Microsoft's own SDK, integrates naturally with C#, and lines up with the AI-200 (Azure AI Cloud Developer Associate) certification direction discussed separately.
- **Container Apps over raw AKS as the primary host**: AKS costs money to run continuously; Container Apps gives the "containerized app running in Azure, deployed via IaC" story for $0, which is 90% of the interview-relevant substance.
- **GitHub Models instead of Azure OpenAI**: same Semantic Kernel code path, zero cost, swappable later if a paid Azure OpenAI resource is ever justified.

## Running locally

```bash
dotnet run --project src/AskMyResume.Api
```

## Build plan

### Milestone 1 — MVP (Bicep + AI + containers + CI/CD)
1. Scaffold the ASP.NET Core minimal API with a single `/chat` endpoint.
2. Add Semantic Kernel; write embedding + retrieval logic over a small set of resume/portfolio text files (start with plain text files in the repo — no vector DB needed yet, in-memory cosine similarity over a handful of documents is enough to be legitimate RAG at this scale).
3. Wire the LLM call through GitHub Models' free tier.
4. Write the Dockerfile; confirm it runs locally (`docker run`).
5. Write `infra/main.bicep` defining Container App + Container App Environment + Log Analytics + Key Vault.
6. Deploy manually once via `az deployment group create -f infra/main.bicep` to confirm it works.
7. Write the GitHub Actions workflow to build, push to GHCR, and re-run the Bicep deployment on push to main.
8. **Done when:** the chatbot is reachable at a public Container Apps URL, answers questions about Martin's resume correctly, and the whole deploy is reproducible from a clean Azure subscription by running the pipeline.

### Milestone 2 — Stretch: Kubernetes
1. Write `k8s/deployment.yaml`, `service.yaml`, `configmap.yaml` for the same container.
2. Get it running on a local `kind` cluster (`kind create cluster`, `kubectl apply -f k8s/`). Iterate here freely — it's free and disposable.
3. Once the manifests work locally, spin up a real AKS cluster using the Azure free trial credit (`az aks create`), apply the same manifests, confirm it works, capture screenshots and `kubectl get pods/svc` output.
4. Tear the cluster down the same day (`az group delete`).
5. **Done when:** there's a portfolio writeup (screenshots + short README section) showing the app running on real AKS, with the manifests committed to the repo for anyone to reproduce.

## Definition of "resume-worthy"

Before listing Bicep / Kubernetes / applied-AI as skills off the back of this project, Martin should be able to, without notes:
- Explain why Container Apps was chosen over AKS for the primary deployment (cost/complexity trade-off)
- Walk through what the Bicep file provisions and why each resource is there
- Explain how the RAG retrieval works (embeddings, similarity search, why chunking matters)
- Debug a failure in any layer (container won't start, Bicep deployment fails, K8s pod crash-loops) without re-deriving it from an AI tool in the moment

If any of those aren't comfortable yet, that's a signal to keep iterating before the skill goes on the CV, not to skip it.

## Prerequisites / one-time setup
- Azure free account (for the $200/30-day trial credit, used only for the AKS excursion)
- Azure CLI (`az`) + Bicep CLI (`az bicep install`)
- Docker Desktop
- `kind` or `minikube` + `kubectl`
- GitHub Models access (github.com/marketplace/models) for a free API token
- .NET SDK (already installed, used daily)

## Open decisions (revisit when picked back up)
- Vector store: starting with in-memory/naive similarity search over a handful of files is fine for MVP; only reach for something like Azure AI Search or a hosted vector DB if the free tier can absorb it — check before adding.
- Whether to seed the RAG corpus from resume text only, or also pull in GitHub repo READMEs for a richer demo — nice-to-have, not required for Milestone 1.
