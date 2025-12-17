import json

class JSONManipulator:
    def serialize(self, data: dict) -> str:
        return json.dumps(data)

    def deserialize(self, json_str: str) -> dict:
        return json.loads(json_str)
