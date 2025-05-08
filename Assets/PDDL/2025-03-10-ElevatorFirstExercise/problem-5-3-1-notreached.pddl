(define (problem pb1)
  (:domain elevator)
  (:objects
    personA personB personC personD personE - person
    floor1 floor2 floor3 - floor
    elevatorX - elevator

  )
  (:init
    (at-person personA floor1)
    (at-person personB floor1)
    (at-person personC floor2)
    (at-person personD floor1)
    (at-person personE floor3)


    (at-elevator elevatorX floor3)
    (above floor3 floor2)
    (above floor2 floor1)
    
  )
  (:goal
    (and
      (at-person personA floor2)
      (at-person personB floor3)
      (at-person personC floor3)
      (at-person personD floor2)
      (at-person personE floor1)
    )
  )
)