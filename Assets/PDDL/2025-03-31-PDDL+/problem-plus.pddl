(define (problem pb1)
  (:domain robot)
  (:objects
    gardenA gardenB - garden
    ball1 ball2 - obj
    wally - robot
  )
  (:init
    (at-robot wally gardenA)
    (free wally)
    (at-obj ball1 gardenA)
    (at-obj ball2 gardenA)
    (path gardenA gardenB)
    (path gardenB gardenA)
    (= (speed wally) 1)
    (= (battery wally) 100)
    (= (distance gardenA gardenB) 30)
    (= (distance gardenB gardenA) 30)
  )

  (:goal
    (and (at-obj ball1 gardenB) (at-obj ball2 gardenB))
  )

)
