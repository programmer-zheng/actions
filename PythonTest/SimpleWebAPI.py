from flask import Flask
import os

app = Flask(__name__)


@app.route("/")
def root():
    return "Hello World"


@app.route("/hello/{name}")
def say_hello(name: str):
    return {"message": f"Hello {name}"}


if __name__ == "__main__":
    port = int(os.environ.get('PORT', 5000))
    app.run(debug=True, host='0.0.0.0', port=port)
