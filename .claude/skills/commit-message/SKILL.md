---
name: commit-message
description: Redacta un mensaje de commit listo para copiar a partir de los cambios en staged (git diff --cached). Usar cuando el usuario pida un mensaje de commit, "mensaje para lo staged" o invoque /commit-message. Nunca hace commit.
---

# Mensaje de commit desde staged

Objetivo: entregar un mensaje de commit listo para copiar que describa **solo** lo que está en staged. No crear commits ni modificar el índice.

## Pasos

1. Leer el staged (solo lectura):
   - `git diff --cached --stat`
   - `git diff --cached` (si es muy grande, revisar por archivo con `git diff --cached -- <ruta>`)
2. Si no hay nada en staged, decirlo en una línea y detenerse. No usar los cambios sin stagear ni sugerir `git add`.
3. Identificar la intención principal del cambio (el porqué), no la lista de archivos.
4. Redactar el mensaje con el formato de abajo.

## Formato

- En inglés, conventional commits: `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.
- Título ≤ 72 caracteres, en minúscula, sin punto final.
- Línea en blanco y luego una descripción de una o dos frases (≤ 150 caracteres) sobre el porqué. Sin listar archivos.
- Sin líneas de atribución ni `Co-Authored-By`.
- Si el staged mezcla cambios sin relación, elegir el tipo del cambio dominante y mencionarlo en una línea fuera del bloque (sugerir dividirlo es opcional).

## Prohibido

- `git add`, `git commit`, `git reset`, `git restore --staged` o cualquier comando que altere el índice o el historial.

## Salida

Solo el mensaje dentro de un bloque de código `text`, sin explicación adicional salvo el aviso de cambios mezclados:

```text
feat: add per-game stats page with player leaderboard

Lets players see win totals across all sessions of a game without opening each session.
```
