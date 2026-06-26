# exception

class RekNNException(RuntimeError):
    def __init__(self, arg=""):
        self.arg = arg
