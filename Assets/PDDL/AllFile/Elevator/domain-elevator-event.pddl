; Now the elevator has both capacity and weight restrictions

(define (domain domain-elevator-event)
  (:requirements :strips :typing :fluents :time :negative-preconditions 
:timed-initial-literals)
  (:types
    person elevator
  )
  (:predicates
    (reached ?p - person)
    (in ?e - elevator ?p - person)
    (movingUp ?e - elevator)
    (movingDown ?e - elevator)
  ) 
  (:functions
    (at-elevator ?e - elevator)
    (at-person ?p - person)
    (target ?p - person)
    (weight ?p - person)
    (capacity ?e - elevator)
    (max-load ?e - elevator)
    (passengers ?e - elevator)
    (load ?e - elevator)
    (distance-run ?e -elevator)
    (floors)
  )

  (:action startMovingUp  
    :parameters(?e - elevator)
    :precondition(and
      (not (movingUp ?e)) 
      (< (at-elevator ?e) (floors))
      )
    :effect(and
      (movingUp ?e)
      (assign (distance-run ?e) 0)
      )
  )

  (:action startMovingDown  
    :parameters(?e - elevator)
    :precondition(and
        (not (movingDown ?e)) 
        (> (at-elevator ?e) 0)
      )
    :effect(and
        (movingDown ?e)
        (assign (distance-run ?e) 0)
      )
  )

  (:process movingProcess
    :parameters(?e - elevator)
    :precondition (or
      (movingUp ?e)
      (movingDown ?e)
    )
    :effect (and
      ;(increase (distance-run ?e) (* #t 10))
      (increase (distance-run ?e) (* #t (- 10 (* 3 (/ (load ?e) (max-load ?e))))) )
      
      ;(increase (distance-run ?e) (* #t (* 0.1 (- 110 (distance-run ?e)))) )
    )
  )

  (:event stopMovingUp
    :parameters(?e - elevator)
    :precondition (and
      (movingUp ?e)
      (>= (distance-run ?e) 100)
    )
    :effect(and
      (not (movingUp ?e))
      (increase (at-elevator ?e) 1)
    )
  )

  (:event stopMovingDown
    :parameters(?e - elevator)
    :precondition (and
      (movingDown ?e)
      (>= (distance-run ?e) 100)
    )
    :effect(and
      (not (movingDown ?e))
      (increase (at-elevator ?e) -1)
    )
  )

  (:action load
    :parameters (?p - person ?e - elevator)
    :precondition (and
      (= (at-person ?p) (at-elevator ?e))
      (<= (+ (passengers ?e) 1) (capacity ?e))
      (<= (+ (load ?e) (weight ?p)) (max-load ?e)) ;Now the weight is restricted
    )
    :effect (and
      (in ?e ?p)
      (assign (at-person ?p) -1)
      (increase (passengers ?e) 1)
      (increase (load ?e) (weight ?p)) ;The weight is added when a passengers enters 
    )
  )

  (:action unload
    :parameters (?p - person ?e - elevator)
    :precondition (and
      (in ?e ?p)
    )
    :effect (and
      (not (in ?e ?p))
      (assign (at-person ?p) (at-elevator ?e))
      (decrease (passengers ?e) 1)
      (decrease (load ?e) (weight ?p)) ;The weight is removed when a passengers exits 
    )
  )

  (:action reached
    :parameters (?p - person)
    :precondition (and
      (= (at-person ?p) (target ?p))
      (not (reached ?p))
    )
    :effect (and
      (reached ?p)
    )
  )

)

