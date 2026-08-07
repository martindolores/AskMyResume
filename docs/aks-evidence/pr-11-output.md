# PR-11: Real AKS deployment evidence

Captured 2026-08-07T11:31:28Z against a real Azure Kubernetes Service cluster
(resource group `askmyresume-aks-rg`, cluster `askmyresume-aks`, single `Standard_D2s_v7` node, Free control-plane tier).
Cluster was created and deleted the same day using Azure free trial credit only.

```
$ kubectl get nodes -o wide
NAME                                STATUS   ROLES    AGE     VERSION   INTERNAL-IP   EXTERNAL-IP   OS-IMAGE             KERNEL-VERSION     CONTAINER-RUNTIME
aks-nodepool1-12071592-vmss000000   Ready    <none>   2m56s   v1.35.6   10.224.0.4    <none>        Ubuntu 24.04.4 LTS   6.8.0-1063-azure   containerd://2.3.3-2

$ kubectl get pods -o wide
NAME                           READY   STATUS    RESTARTS   AGE   IP             NODE                                NOMINATED NODE   READINESS GATES
askmyresume-667f79c569-nkgj5   1/1     Running   0          56s   10.244.0.253   aks-nodepool1-12071592-vmss000000   <none>           <none>

$ kubectl get svc
NAME          TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE
askmyresume   ClusterIP   10.0.159.123   <none>        80/TCP    55s
kubernetes    ClusterIP   10.0.0.1       <none>        443/TCP   4m27s

$ kubectl get deployment askmyresume -o wide
NAME          READY   UP-TO-DATE   AVAILABLE   AGE   CONTAINERS    IMAGES                                     SELECTOR
askmyresume   1/1     1            1           57s   askmyresume   ghcr.io/martindolores/askmyresume:latest   app=askmyresume
```

## Live /chat call through the Service (port-forwarded)

```
$ curl -X POST http://localhost:18080/chat -H "Content-Type: application/json" -d '{"question":"What skills does this project demonstrate?"}'
{
    "answer": "Based on the context provided, no single specific project is named as \"this project,\" but the projects and achievements mentioned demonstrate the following skills:\n\n* **Mobile Application Project (Interfi Systems):**\n  * Software architecture and stack/structural decision-making (sole architect)\n  * Mobile application development using **Capacitor**\n  * End-to-end feature shipping\n\n* **Design & Alignment Project/Role (Retail Directions):**\n  * Software design and producing design artifacts (mockups, class diagrams, and flow diagrams)\n  * Using **Microsoft Visio**\n  * Stakeholder alignment between engineering and product teams\n\n* **Client Integration Project:**\n  * Performance optimization and latency reduction (cutting response time from ~15 minutes to seconds)\n\n* **CI/CD Optimization Project:**\n  * CI pipeline optimization and **test sharding** (halving CI runtime)\n\n*(Note: Broadly across his work, he also demonstrates full-stack software engineering with React, TypeScript, C#/.NET, REST APIs, GraphQL, Cloud/CI tools like Azure, Docker, GitHub Actions, and directing agentic AI coding tools like Claude Code).*"
}
```
