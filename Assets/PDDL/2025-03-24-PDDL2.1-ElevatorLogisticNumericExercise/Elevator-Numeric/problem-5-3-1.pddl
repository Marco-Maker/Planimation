(define (problem pb1)
  (:domain elevator)
  (:objects
    personA personB personC personD personE - person
    elevatorX - elevator

  )
  (:init
    (= (floors) 3)

    (= (at-person personA) 1)
    (= (target personA) 2)
    (= (at-person personB) 1)
    (= (target personB) 3)
    (= (at-person personC) 2)
    (= (target personC) 3)
    (= (at-person personD) 1)
    (= (target personD) 2)
    (= (at-person personE) 3)
    (= (target personE) 1)

    (= (at-elevator elevatorX) 3)
    (= (passengers elevatorX) 0)
    (= (capacity elevatorX) 2)
  )
  (:goal
    (and
      (reached personA)
      (reached personB)
      (reached personC)
      (reached personD)
      (reached personE)
    )
  )
)