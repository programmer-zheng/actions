FROM python:3.11.3
#FROM python:3.11.1-alpine
EXPOSE 5000
WORKDIR /app
#COPY ./SimpleWebAPI.py .
#COPY ./requirements.txt .
COPY . .

RUN pip install -r requirements.txt
#ENTRYPOINT python -m uvicorn main:app --reload

ENTRYPOINT [ "python" ]
CMD ["SimpleWebAPI.py" ]