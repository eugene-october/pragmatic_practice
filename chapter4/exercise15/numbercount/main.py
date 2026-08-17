N = 100


def number_count(n: int) -> int:
    count = 0
    for i in range(0, n + 1, 5):
        count += 1

    return count


print(f"---number_count(N)---{number_count(N)}")
