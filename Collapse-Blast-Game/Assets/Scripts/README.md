# Collapse/Blast Game - Unity Setup Guide

## Sahne Kurulumu (Scene Setup)

### 1. GameManager GameObject Oluşturma

```
Hierarchy:
├── GameManager (Empty GameObject)
│   ├── GameController (Script)
│   ├── GridManager (Script)
│   ├── BlockPool (Script)
│   └── InputHandler (Script)
├── Main Camera
└── GridParent (Empty GameObject)
```

### 2. ScriptableObject'leri Oluşturma

1. **GameConfig Oluşturma:**
   - Assets > Create > CollapseBlast > Game Config
   - Değerleri ayarla: M=10, N=12, K=6, A=4, B=7, C=9

2. **BlockColorData Oluşturma (6 adet):**
   - Assets > Create > CollapseBlast > Block Color Data
   - Her renk için bir tane oluştur (Blue, Green, Pink, Purple, Red, Yellow)
   - `Match_Items_internship` klasöründeki ikonları ata

### 3. Block Prefab Oluşturma

1. Yeni bir Empty GameObject oluştur, ismi "Block" yap
2. Ekle: SpriteRenderer, BoxCollider2D
3. Ekle: BlockView Script
4. Assets/Prefabs klasörüne sürükle

### 4. Referansları Bağlama

**GameController:**
- Game Config → GameConfig asset
- Grid Manager → GridManager component
- Input Handler → InputHandler component

**GridManager:**
- Game Config → GameConfig asset
- Block Pool → BlockPool component
- Block Prefab → Block prefab
- Color Data Array → 6 adet BlockColorData

**InputHandler:**
- Main Camera → Main Camera
- Block Layer → Block layer (opsiyonel)

### 5. Kamerayı Ayarlama

- Camera → Projection = Orthographic
- Clear Flags = Solid Color
- Background = İstediğiniz renk

---

## Parametreler

| Parametre | Açıklama | Aralık |
|-----------|----------|--------|
| M (rowCount) | Satır sayısı | 2-10 |
| N (columnCount) | Sütun sayısı | 2-10 |
| K (colorCount) | Renk sayısı | 1-6 |
| A (thresholdA) | 1. ikon eşiği | >=2 |
| B (thresholdB) | 2. ikon eşiği | > A |
| C (thresholdC) | 3. ikon eşiği | > B |

---

## Test Senaryoları

1. **Temel Test:** M=5, N=8, K=4, A=4, B=6, C=8
2. **Büyük Grid:** M=10, N=12, K=6, A=4, B=7, C=9
3. **Deadlock Testi:** K=1 (tek renk - anında deadlock tetikler)
