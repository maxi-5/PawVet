# Etapa 1: compilar el cliente React
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json ./
RUN npm install
COPY . ./
ARG VITE_API_CLINICA=http://localhost:5001/api/v1
ARG VITE_API_AGENDA=http://localhost:5002/api/v1
ENV VITE_API_CLINICA=$VITE_API_CLINICA
ENV VITE_API_AGENDA=$VITE_API_AGENDA
RUN npm run build

# Etapa 2: servir los archivos estaticos con nginx
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
