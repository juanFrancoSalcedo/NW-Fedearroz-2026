---
name: uservice
description: Global infrastructure services that live the entire game lifetime. Non-MonoBehaviour, single responsibility. Examples include, SaveService
---

# Services (Infraestructura Global)


Ejemplos:

- SaveService

- **Persistencia**: No debe depender de objetos específicos de una escena, no deben heredar de monobehavioure.
- **Single Responsibility**: Un servicio solo hace una cosa (ej. `SaveService` solo guarda/carga).
---