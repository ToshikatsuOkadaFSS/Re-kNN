# Sample Application for Python

This document explains the library and sample program for using Re-kNN in Python.

## Prerequisites

### Environment

It has been tested on Python 3.12. There may be issues depending on the combination of libraries used.  
Please make modifications as needed to fit your environment.

### Package

Currently, it is provided as an evaluation version, so installation from a local folder is required.

### License

The sample code and documentation are provided under the **Apache License 2.0**.

## Preparation

Please install the package from the command line.

> pip install -e <reknn-path>

Example:  
> pip install -e ./reknn

## Example Code

Use it as follows:

```Python
from reknn import ReKNN

model = ReKNN('bert')
```

## About the Sample Program

This sample program tokenizes and searches text files located under a specified folder.  
It is merely a sample code, so please create your own program with this as a reference.

### How to Use

#### Items to Modify

Please edit the following descriptions according to your environment.  
In this sample, Japanese is set as `ja`, English as `en`.  
Also, specify the folder storing the original text files in `basePath`.  
The sample program does not support file deletion.

##### Environment Setup

```Python
if lang_mode == 'ja':
    model_name = "tohoku-nlp/bert-base-japanese-v3"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-ja'
    dict_path = 'textDict-ja.json'
    basePath = Path("/local/tokada/jawikiout-txt")

elif lang_mode == 'en':
    #model_name = "./bert-base-uncased"
    model_name = "google-bert/bert-base-uncased"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-en'
    dict_path = 'textDict-en.json'
    basePath = Path("/local/tokada/enwikiout-txt")
else:
    print(f"bad lang_mode={lang_mode}")
    exit()
```

##### Test Operation

This section sets the search key. Only code for `ja` is described.  
Please modify as needed.

```Python
elif mode == 'test':
    if lang_mode == 'ja':
        for keywords in ['週刊少年サンデー', '聖☆おにいさん']:
            print("##### keywords = {0} #####".format(keywords))
            print("### Search ###")
            results = search(model, bert_model, keywords, 5, text_rev_dict)
            print("### Explain ###")
            results = explain(model, bert_model, keywords, 5, text_rev_dict)
    elif lang_mode == 'en':
        pass
```

### Limitations

Currently, the following limitations exist; we plan to address them sequentially:

- Deletion/Update of indexed data  
- Output of cluster information

