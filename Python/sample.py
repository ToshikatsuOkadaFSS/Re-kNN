import ctypes
import os
import sys
import numpy as np
from tqdm import tqdm
import json
from pathlib import Path
from datetime import datetime
import torch
from transformers import AutoTokenizer, AutoModel
from reknn import ReKNN
import time


def toVectorSub(inputs):
    # 4. モデルに投入して、ベクトル（隠れ状態）を抽出
    with torch.no_grad():  # 評価・推論時は勾配の計算をオフにしてメモリを節約
        outputs = model(**inputs)
    
    # 最終層の出力（Last Hidden State）を取得
    # 形状（Shape）は [バッチサイズ(1), トークン数, ベクトルの次元数(768)]
    last_hidden_state = outputs.last_hidden_state
    
    # 扱いやすいようにバッチの次元を削り、[トークン数, 768] の形状にする
    token_embeddings = last_hidden_state[0]
    
    # 5. 特殊トークン（[CLS], [SEP]）を含んだ、実際のトークン文字のリストを取得
    tokens = tokenizer.convert_ids_to_tokens(inputs["input_ids"][0])

#    # 6. トークンと対応するベクトルを紐づけて表示
#    print(f"入力文章: {text}")
#    print(f"トークン総数: {len(tokens)}\n")
#    print(f"{'トークン':<10} | {'ベクトルの形状':<12} | {'ベクトルの先頭3要素'}")
#    print("-" * 60)
#    
#    for token, embedding in zip(tokens, token_embeddings):
#        # 表示用にベクトルの先頭3要素だけをリスト化
#        clip_embedding = embedding[:3].tolist()
#        # 読みやすさのために数値を丸めて表示
#        formatted_emb = [round(v, 4) for v in clip_embedding]
#        print(f"{token:<12} | {str(list(embedding.shape)):<14} | {formatted_emb}...")
    
    return tokens, token_embeddings


def toVector(bert_model, text):

    #start = time.perf_counter()

    #print("toVector : text = {0}".format(text))

    # 1. 特殊トークンを入れずに、一度全体をトークンIDのリストに変換
    all_input_ids = tokenizer.encode(text, add_special_tokens=False)

    if len(all_input_ids) == 0:
        return None, None
    

    # BERTの上限512から、[CLS]と[SEP]の2面分を引いた「510」を1チャンクの最大値にする
    max_chunk_size = 510

    # 2. 510トークンごとにIDのリストを分割
    chunks = [all_input_ids[i:i + max_chunk_size] for i in range(0, len(all_input_ids), max_chunk_size)]

    if len(chunks) > 1:
        print("chunk size = {0}".format(len(chunks)))
        print(text)

    all_chunk_embeddings = []
    all_tokens = []
    
    #print(f"全体トークン数: {len(all_input_ids)}")
    #print(f"分割されたチャンク数: {len(chunks)}\n")
    
    # 3. 各チャンクごとにBERTでベクトル化
    for i, chunk in enumerate(chunks):
        # 特殊トークン([CLS], [SEP])を付加したIDリストを作成
        if lang_mode == 'ja':
            input_ids_with_special = tokenizer.build_inputs_with_special_tokens(chunk)
        else:
            input_ids_with_special = chunk
        
        # テンソル形式に変換
        inputs = {
            "input_ids": torch.tensor([input_ids_with_special]),
            "attention_mask": torch.tensor([[1] * len(input_ids_with_special)])
        }
        
        with torch.no_grad():
            outputs = bert_model(**inputs)
        
        # 最終層の出力を取得 [1, シーケンス長, 768]
        last_hidden_state = outputs.last_hidden_state[0]
        
        # 【重要】結合した時に邪魔になるため、先頭の[CLS]と末尾の[SEP]のベクトルを削る
        # [1:-1] で特殊トークン以外の純粋なトークンベクトルを抽出
        pure_chunk_embeddings = last_hidden_state[1:-1]
        
        # 結果をストック
        all_chunk_embeddings.append(pure_chunk_embeddings)
        all_tokens.extend(tokenizer.convert_ids_to_tokens(chunk))
        
        #print(f"チャンク {i+1} 処理完了: トークン数 {len(chunk)}")

    # 4. すべてのチャンクのベクトルを縦方向に結合
    # 形状は [全体の純粋なトークン数, 768] になる
    total_embeddings = torch.cat(all_chunk_embeddings, dim=0)

    #print("\n--- 最終結果 ---")
    #print(f"結合後のトークン配列の形状: {total_embeddings.shape}")
    #print(f"復元されたトークン総数: {len(all_tokens)}")    
    
    #if len(chunks) > 1:
    #    print("tokens = {0}".format(",".join(all_tokens)))
    #    print(text)

    #end = time.perf_counter()

    #print('time = {:.2f}s'.format(end-start))
    
    return all_tokens, total_embeddings


def check_delete(model, basePath, textDict, revDict):
    for relPath in list(textDict.keys()):
        chkPath = basePath / relPath

        if not chkPath.is_file():
            print("delete : {0}".format(relPath))
            fileId = textDict[relPath][0]
            model.delete(fileId)
            del textDict[relPath]
            del revDict[fileId]

    
def add(model, bert_model, basePath, relPath, textDict, count, maxData, searchMax, threshold):

    #print(f"{count}:{relPath}")

    if count >= maxData:
        return count

    if relPath is None:
        chkPath = basePath
    else:
        chkPath = basePath / relPath

    # フォルダ（ディレクトリ）の一覧を取得
    folders = sorted([f.name for f in chkPath.iterdir() if f.is_dir() and not f.name.startswith('.')])
    
    # ファイルの一覧を取得
    files = sorted([f.name for f in chkPath.iterdir() if f.is_file() and not f.name.startswith('.') and f.name.endswith('.txt')])

    for folder in folders:
        if relPath is None:
            newRelPath = Path(folder)
        else:
            newRelPath = relPath / folder

        count = add(model, bert_model, basePath, newRelPath, textDict, count, maxData, searchMax, threshold)

        if count >= maxData:
            break

    prev = int(model.get_total_vector(model.current_instance))
    add_vector_set = {}
    key_set = []

    if count >= maxData:
        return count

    for file in files:
        if count >= maxData:
            break

        srcPath = basePath / relPath / file
        relFile = relPath / file
        relFileStr = str(relFile)

        if relFileStr in textDict.keys():
            fileId = textDict[relFileStr][0]
            #print(
            #    srcPath.stat().st_mtime,
            #    textDict[relFileStr][1],
            #    srcPath.stat().st_size,
            #    textDict[relFileStr][2])
            if srcPath.stat().st_mtime == textDict[relFileStr][1] and srcPath.stat().st_size == textDict[relFileStr][2]:
                #print(f"same : {count}:{relFileStr}")
                count += 1
                continue
            delSize = model.delete(fileId)
            print(f"Update {relFileStr} : {delSize}")
            prev -= delSize
        else:
            if len(textDict) > 0:
                #print(textDict.values())
                fileId = max(textDict.values(), key=lambda x: int(x[0]))[0] + 1
            else:
                fileId = 0
            print(relFileStr)

        #print(fileId)

        textDict[relFileStr] = [fileId, srcPath.stat().st_mtime, srcPath.stat().st_size]

        add_vector_set[fileId] = {}

        with open(srcPath, mode='r', encoding='utf-8') as ifp:
            key_set_sub = []
            for lineNo, line in enumerate(ifp):
                line = line.rstrip('\n')
                #print(line)
                
                tokens, token_embeddings = toVector(bert_model, line)

                if tokens is None:
                    continue
                if token_embeddings is None:
                    continue

                vec = token_embeddings.numpy()

                add_vector_set[fileId][lineNo] = [
                        tokens, 
                        vec, 
                        np.full(vec.shape[0], fileId, dtype=np.int32),
                        np.full(vec.shape[0], lineNo, dtype=np.int32)]

                key_set_sub += [[fileId, lineNo]]
                #if lineNo == 19:
                #    print(fileId, lineNo)

        key_set += key_set_sub

        count += 1

        if count % 10 == 0:
            # 一旦 flush する
            if len(key_set) > 0:
                add_vector = np.concatenate([add_vector_set[x][y][1] for x,y in key_set], axis=0)
                add_fileIds = np.concatenate([add_vector_set[x][y][2] for x,y in key_set], axis=0)
                add_lineInfos = np.concatenate([add_vector_set[x][y][3] for x,y in key_set], axis=0)

                if int(model.get_total_vector(model.current_instance)) == 0:
                    #print("fit")
                    #model.fit(vec, np.full(vec.shape[0], fileId, dtype=np.int32), np.full(vec.shape[0], lineNo, dtype=np.int32))
                    model.fit(add_vector, add_fileIds, add_lineInfos)
                else:
                    #print("add")
                    #model.add(vec, np.full(vec.shape[0], fileId, dtype=np.int32), np.full(vec.shape[0], lineNo, dtype=np.int32))
                    model.add(add_vector, add_fileIds, add_lineInfos)

            now = datetime.now()
            print(f"{now.strftime('%Y-%m-%d %H:%M:%S')}:{count}:total vector = {model.get_total_vector()}")

            after = int(model.get_total_vector(model.current_instance))

            #print(f"prev = {prev}, after={after}, add={vec.shape[0]}")
            if ( prev + add_vector.shape[0]) != after:
                print(f"prev = {prev}, after={after}, add={add_vector.shape[0]}")
                print(f"SIZE ERROR!!!")
                
            prev = int(model.get_total_vector(model.current_instance))
            add_vector_set = {}
            key_set = []


        if count >= maxData:
            break

    if len(key_set) > 0:
        add_vector = np.concatenate([add_vector_set[x][y][1] for x,y in key_set], axis=0)
        add_fileIds = np.concatenate([add_vector_set[x][y][2] for x,y in key_set], axis=0)
        add_lineInfos = np.concatenate([add_vector_set[x][y][3] for x,y in key_set], axis=0)

        if int(model.get_total_vector(model.current_instance)) == 0:
            #print("fit")
            #model.fit(vec, np.full(vec.shape[0], fileId, dtype=np.int32), np.full(vec.shape[0], lineNo, dtype=np.int32))
            model.fit(add_vector, add_fileIds, add_lineInfos)
        else:
            #print("add")
            #model.add(vec, np.full(vec.shape[0], fileId, dtype=np.int32), np.full(vec.shape[0], lineNo, dtype=np.int32))
            model.add(add_vector, add_fileIds, add_lineInfos)

        now = datetime.now()
        print(f"{now.strftime('%Y-%m-%d %H:%M:%S')}:{count}:total vector = {model.get_total_vector()}")

        after = int(model.get_total_vector())

        #print(f"prev = {prev}, after={after}, add={vec.shape[0]}")
        if ( prev + add_vector.shape[0]) != after:
            print(f"prev = {prev}, after={after}, add={add_vector.shape[0]}")
            print(f"SIZE ERROR!!!")

    #print(f"total vec={model.get_total_vector()}")
        
    #print(f"return {count}")
    return count    


def search(model, bert_model, text, max_line, text_rev_dict):

    tokens, token_embeddings = toVector(bert_model, text)

    if tokens is None:
        return None
    if token_embeddings is None:
        return None        

    vec = token_embeddings.numpy()

    #print(vec.shape)

    result = model.search(vec)

    #print(result)

    print("\n<<< Result (by document) >>>")
    count = 0
    for id, score in sorted([[x, result['ResultItemMains'][x]] for x in result['ResultItemMains'].keys()], key=lambda x: x[1], reverse=True)[0:5]:
        fname = text_rev_dict[id]
        print("### Rank : {0}".format(count))
        print("### {0} : Score {1}".format(fname, score))
        fpath = basePath / fname
        with open(fpath, mode='r', encoding='utf-8') as ifp:
            lineNo = 0
            for line in ifp:
                #line = line.rstrip('\n')
                print(line.rstrip('\n'))
                lineNo += 1
                if lineNo >= max_line:
                    break
        count += 1

    print("\n<<< Result Detail (by document) >>>")
    for i,word in enumerate(tokens):
        print(f"#### word = {word} ####")
        count = 0
        for id, score in sorted(
            [
                [
                    x, 
                    result['ResultItemMainDetails'][i][x],
                ] for x in result['ResultItemMainDetails'][i].keys()
            ],
            key=lambda x: x[1],
            reverse=True
        ):
            print(id, score)
            fname = text_rev_dict[id]
            print("### Rank : {0}".format(count))
            print("### {0} : Score {1}".format(fname, score))
            fpath = basePath / fname
            with open(fpath, mode='r', encoding='utf-8') as ifp:
                lineNo = 0
                for line in ifp:
                    #line = line.rstrip('\n')
                    print(line.rstrip('\n'))
                    lineNo += 1
                    if lineNo >= max_line:
                        break
            count += 1

    print("\n<<< Result (by document-sub) >>>")
    count = 0
    for id, subId, score in sorted(
            [
                [
                    x,
                    result['ResultItemMainAndSubs'][x][0],
                    result['ResultItemMainAndSubs'][x][1],
                ]
                for x in result['ResultItemMainAndSubs'].keys()
            ], 
            key=lambda x: x[2], 
            reverse=True)[0:5]:
        fname = text_rev_dict[id]
        print("### Rank : {0}".format(count))
        print("### {0}, {1} : Score {2}".format(fname, subId, score))
        fpath = basePath / fname
        with open(fpath, mode='r', encoding='utf-8') as ifp:
            lineNo = 0
            for line in ifp:
                if lineNo == subId:
                    #line = line.rstrip('\n')
                    print(line.rstrip('\n'))
                    break
                lineNo += 1
        count += 1

    print("\n<<< Result Detail(by document-sub) >>>")
    for i,word in enumerate(tokens):
        print(f"#### word = {word} ####")
        count = 0
        for id, subId, score in sorted(
            [
                [
                    x,
                    result['ResultItemMainAndSubDetails'][i][x][0],
                    result['ResultItemMainAndSubDetails'][i][x][1],
                ] 
                for x in result['ResultItemMainAndSubDetails'][i].keys()
            ],
            key=lambda x: x[2],
            reverse=True
        ):
            print(id, score)
            fname = text_rev_dict[id]
            print("### Rank : {0}".format(count))
            print("### {0}, {1} : Score {2}".format(fname, subId, score))
            fpath = basePath / fname
            with open(fpath, mode='r', encoding='utf-8') as ifp:
                lineNo = 0
                for line in ifp:
                    if lineNo == subId:
                        #line = line.rstrip('\n')
                        print(line.rstrip('\n'))
                        break
                    lineNo += 1
            count += 1

    return result



def predict(model, bert_model, text):

    tokens, token_embeddings = toVector(bert_model, text)

    if tokens is None:
        return None
    if token_embeddings is None:
        return None        

    vec = token_embeddings.numpy()

    print(vec.shape)

    return model.predict(vec)


def explain(model, bert_model, text, max_line, text_rev_dict):

    tokens, token_embeddings = toVector(bert_model, text)

    if tokens is None:
        return None
    if token_embeddings is None:
        return None        

    vec = token_embeddings.numpy()

    #print(vec.shape)

    result = model.explain(vec)
    count = 0
    for id, score in sorted([[x, result['ScoreListMains'][x]] for x in result['ScoreListMains'].keys()], key=lambda x: x[1], reverse=True)[0:5]:
        fname = text_rev_dict[id]
        print("### Rank : {0}".format(count))
        print("### {0} : Score {1}".format(fname, score))
        fpath = basePath / fname
        with open(fpath, mode='r', encoding='utf-8') as ifp:
            lineNo = 0
            for line in ifp:
                #line = line.rstrip('\n')
                print(line.rstrip('\n'))
                lineNo += 1
                if lineNo >= max_line:
                    break
        count += 1

    return results

if len(sys.argv) < 3:
    print('usage : python sample.py <lang> <mode> <option>')
    print('example : python sample.py en add 500')
    print('example : python sample.py ja test')
    print('example : python sample.py ja refine')
    elineNoxit()

lang_mode = sys.argv[1]
mode = sys.argv[2]

if lang_mode == 'ja':
    model_name = "tohoku-nlp/bert-base-japanese-v3"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-ja'
    dict_path = 'textDict-ja.json'
    cluster_path = 'cluster-ja.json'
    basePath = Path("/local/tokada/jawikiout-txt")

elif lang_mode == 'en':
    #model_name = "./bert-base-uncased"
    model_name = "google-bert/bert-base-uncased"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-en'
    dict_path = 'textDict-en.json'
    cluster_path = 'cluster-en.json'
    basePath = Path("/local/tokada/enwikiout-txt")
else:
    print(f"bad lang_mode={lang_mode}")
    exit()


model = ReKNN('bert')
model.set_debug_mode(0)

text_rev_dict = {}
if model.load(db_path):
    if Path(dict_path).exists():
        with open(dict_path, "r", encoding="utf-8") as f:
            text_dict = json.load(f)
            for key in text_dict.keys():
                if type(text_dict[key]) is int:
                    fileId = text_dict[key]
                    srcPath = basePath / key
                    text_dict[key] = [fileId, srcPath.stat().st_mtime, srcPath.stat().st_size]
                text_rev_dict[text_dict[key][0]] = key
    
    else:
        model.clear()
        text_dict = {}
else:
    text_dict = {}

print(model.get_total_vector())

if mode == 'add':
    #model.set_debug_mode(1)
    target_num = int(sys.argv[3])

    check_delete(model, basePath, text_dict, text_rev_dict)

    add(model, bert_model, basePath, None, text_dict, 0, target_num, 5, 0.9)
    
    model.save(db_path)
    
    with open(dict_path, "w", encoding="utf-8") as f:
        # ensure_ascii=False にすることで日本語が文字化け（\uXXXX 形式）せずに保存されます
        # indent=4 を指定すると綺麗に改行・インデントされて見やすくなります
        json.dump(text_dict, f, ensure_ascii=False, indent=4)
    
    print(model.get_total_vector())

elif mode == 'test':
    if lang_mode == 'ja':
        for keywords in ['週刊少年サンデー', '聖☆おにいさん']:
        #for keywords in ['元プロバスケットボール選手']:
            print("##### keywords = {0} #####".format(keywords))
            print("### Search ###")
            results = search(model, bert_model, keywords, 5, text_rev_dict)
            print("### Explain ###")
            results = explain(model, bert_model, keywords, 5, text_rev_dict)
    elif lang_mode == 'en':
        pass

elif mode == 'refine':
    print("start refine.")
    model.set_debug_mode(1)
    result = model.refine(3)
    model.save(db_path)
    print(f"refine finished({result}).")

elif mode == 'del':
    print("del file")
    print(model.get_total_vector())

    fileId = 90
    relPath = text_rev_dict[fileId]
    model.delete(fileId)
    del text_dict[relPath]
    del text_rev_dict[fileId]

    model.save(db_path)
    
    with open(dict_path, "w", encoding="utf-8") as f:
        # ensure_ascii=False にすることで日本語が文字化け（\uXXXX 形式）せずに保存されます
        # indent=4 を指定すると綺麗に改行・インデントされて見やすくなります
        json.dump(text_dict, f, ensure_ascii=False, indent=4)
    
    print(model.get_total_vector())

elif mode == 'clustering':
    print("clustering")
    result = model.get_cluster()
    print(len(result))
    with open(cluster_path, "w", encoding="utf-8") as f:
        # ensure_ascii=False にすることで日本語が文字化け（\uXXXX 形式）せずに保存されます
        # indent=4 を指定すると綺麗に改行・インデントされて見やすくなります
        json.dump(result, f, ensure_ascii=False, indent=4)

else:
    print("mode error")


