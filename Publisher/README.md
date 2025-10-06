# Python Publisher pentru Message_Broker_PAD_gRPC

## 1) Copiază `.proto`
Din repo-ul tău (KawaiFunk/Message_Broker_PAD_gRPC), copiază fișierul `.proto` (de ex. `message_broker.proto`) în `proto/`.

> Notă: denumirile serviciului/metodelor pot fi `Broker.Publish(...)`, `PublishRequest`, `PublishReply`. Dacă la tine sunt diferite, **actualizează importurile și câmpurile** în `src/publisher/client.py`.

## 2) Instalează dependențele
```bash
python -m venv .venv && source .venv/bin/activate  # (Windows: .venv\Scripts\activate)
pip install -r requirements.txt
