from hypothesis import given
from hypothesis import strategies as st


@given(st.lists(st.integers()))
def test_reversing_twice_returns_original(values: list[int]) -> None:
    assert list(reversed(list(reversed(values)))) == values


@given(st.integers(0, 49))  # fails with (0, 50)
def test_lesser_than_50(val: int) -> None:
    assert val < 50
