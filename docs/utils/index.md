---
layout: default
title: Утилиты
nav_order: 3
has_children: true
---

# Утилиты

`Domain/Utils/`

Технические абстракции которые не принадлежат ни одной конкретной предметной области но используются повсеместно в домене.

## Разделы

- [StringType]({% link utils/stringtype.md %}) - базовый класс типобезопасных строк
- [Маски]({% link utils/masks.md %}) - DynamicMask и HierarchicalMask
- [Сериализация]({% link utils/serd.md %}) - NodeJsonConverter, StringTypeJsonConverter
- [Генератор случайных чисел]({% link utils/random.md %}) - RandomS