
(define (domain robot)
  (:requirements :typing :fluents :time :negative-preconditions 
:timed-initial-literals)
  (:types
    garden obj robot
  )

  (:predicates
    (at-robot ?r - robot ?l - garden)
    (at-obj ?b - obj ?r - garden)
    (free ?r - robot)
    (moving ?r - robot)
    (charging ?r - robot)
    (moving-path ?r - robot ?a - garden ?b - garden)
    (carry ?o - obj ?r - robot)
    (allowed ?r - robot ?l - garden)
    (path ?a - garden ?b - garden)
  )

  (:functions
    (speed ?r - robot)
    (battery ?r - robot)
    (distance ?a - garden ?b - garden)
    (time)
  )

  (:action pick
    :parameters (?o - obj ?l - garden ?r - robot)
    :precondition (and
      (at-obj ?o ?l)
      (at-robot ?r ?l)
      (free ?r)
    )
    :effect (and
      (carry ?o ?r)
      (not (at-obj ?o ?l))
      (not (free ?r)))
  )

  (:action drop
    :parameters (?o - obj ?l - garden ?r - robot)
    :precondition (and (carry ?o ?r) (at-robot ?r ?l))
    :effect (and (at-obj ?o ?l) (free ?r)
      (not (carry ?o ?r)))
  )

  (:action startMove
    :parameters (?r - robot ?a - garden ?b - garden)
    :precondition (and
      (at-robot ?r ?a)
      (not (moving ?r))
      (not (charging ?r))
    )
    :effect (and
      (not (at-robot ?r ?a))
      (moving ?r)
      (moving-path ?r ?a ?b)
      (assign (moved-distance ?r) 0)
    )
  )

  (:action repriseMovement
    :parameters (?r - robot ?a - garden ?b - garden)
    :precondition (and
      (moving-path ?r ?a ?b)
      (not (moving ?r))
      (not (charging ?r))
    )
    :effect (and
      (moving ?r)
    )
  )

  (:process movingProcess
    :parameters(?r - robot)
    :precondition (and
      (moving ?r)
    )
    :effect (and
      (decrease (battery ?r) (* 2 #t))
      (increase (moved-distance ?r) (* #t (speed ?r)))
    )
  )

  (:event arrived
    :parameters(?r - robot ?a - garden ?b - garden)
    :precondition (and
      (moving ?r)
      (moving-path ?r ?a ?b)
      (>= (moved-distance ?r) (distance ?a ?b))
    )
    :effect(and
      (at-robot ?r ?b)
      (not (moving ?r))
      (not (moving-path ?r ?a ?b))
    )
  )

  (:event batteryDead
    :parameters(?r - robot)
    :precondition (and
      (<= (battery ?r) 10)
      (moving ?r)
    )
    :effect (and
      (not (moving ?r))
    )
  )

  (:action startCharge
    :parameters (?r - robot)
    :precondition (and
      (moving ?r)
      (<= (battery ?r) 50)
    )
    :effect (and
      (not (moving ?r))
      (charging ?r)
    )
  )

  (:process charging
    :parameters(?r - robot)
    :precondition (and
      (charging ?r)
      (<= (battery ?r) 100)
    )
    :effect (and
      (increase
       (battery ?r)
       (* (* (- 100 (battery ?r)) 0.2) #t))
      ; (increase (battery ?r) #t)
    )
  )

  (:action stopCharge
    :parameters (?r - robot)
    :precondition (and
      (>= (battery ?r) 90)
      (charging ?r)
    )
    :effect (and
      (not (charging ?r))
    )
  )

)
