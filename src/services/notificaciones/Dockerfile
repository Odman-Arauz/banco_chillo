# Imagen Docker del microservicio Notificaciones (Node.js + Express) — Parte 15 del práctico.
#   docker build -t banco-chillo/notificaciones src/services/notificaciones
#   docker run --rm -p 5007:8080 --env-file src/services/notificaciones/.env banco-chillo/notificaciones

FROM node:22-alpine
WORKDIR /app

# 1. Copiar solo los manifiestos e instalar dependencias de producción (aprovecha la caché de Docker).
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

# 2. Copiar el código fuente.
COPY src ./src

ENV NODE_ENV=production
ENV PUERTO=8080
EXPOSE 8080

# Usuario sin privilegios incluido en la imagen oficial de Node.
USER node
CMD ["node", "src/server.js"]
