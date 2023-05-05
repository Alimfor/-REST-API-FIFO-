# ProjectHack
Алгоритм для подсчета прибыли за период с учетом себестоимости по FIFO (First in first out)
Оссобенность этого API в том, что она оптимизированна под скорость отклика и нагрузку базы данных.
Для реализации этого было использованно:
1) Асиннхронное программирование
2) Пул соеденения
3) Пакетная обработка данных
4) Кэширование

Также для удобства, есть возможность узнать количество потребляемой памяти базы данных, жесткого диска, а также время отклика.
