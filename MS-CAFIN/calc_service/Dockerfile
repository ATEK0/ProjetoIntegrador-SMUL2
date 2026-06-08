FROM python:3.12-slim

WORKDIR /app

COPY requirements.txt .
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    libc6-dev \
    pkg-config \
    default-libmysqlclient-dev \
    && rm -rf /var/lib/apt/lists/*
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

# Expõe a porta
EXPOSE 8000

# Executa o Gunicorn
CMD ["gunicorn", "calc_service.wsgi:application", "--bind", "0.0.0.0:8000"]
