FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/dnd-tavern/browser /usr/share/nginx/html/browser
COPY nginx.conf /etc/nginx/nginx.conf
RUN rm -f /etc/nginx/conf.d/default.conf  # Удаляем дефолтную конфигурацию
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]